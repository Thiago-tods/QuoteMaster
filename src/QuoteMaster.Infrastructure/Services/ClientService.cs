using Microsoft.EntityFrameworkCore;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Domain.Entities;
using QuoteMaster.Domain.Rules;
using QuoteMaster.Infrastructure.Data;

namespace QuoteMaster.Infrastructure.Services;

public sealed class ClientService(ApplicationDbContext db) : IClientService
{
    public async Task<ClientDto> CreateAsync(SaveClientDto input, CancellationToken cancellationToken = default)
    {
        Validate(input);
        var client = new Client
        {
            Name = input.Name.Trim(),
            Document = Normalize(input.Document),
            Email = Normalize(input.Email),
            Phone = Normalize(input.Phone),
            Address = Normalize(input.Address)
        };
        db.Clients.Add(client);
        await db.SaveChangesAsync(cancellationToken);
        return Map(client);
    }

    public async Task<ClientDto> UpdateAsync(int id, SaveClientDto input, CancellationToken cancellationToken = default)
    {
        Validate(input);
        var client = await db.Clients.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Cliente não encontrado.");
        client.Name = input.Name.Trim();
        client.Document = Normalize(input.Document);
        client.Email = Normalize(input.Email);
        client.Phone = Normalize(input.Phone);
        client.Address = Normalize(input.Address);
        await db.SaveChangesAsync(cancellationToken);
        return Map(client);
    }

    public async Task<ClientDto?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return client is null ? null : Map(client);
    }

    public async Task<IReadOnlyList<ClientDto>> SearchAsync(string? search, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = db.Clients.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || (x.Document != null && x.Document.Contains(term)) || (x.Email != null && x.Email.Contains(term)));
        }
        var rows = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return rows.Select(Map).ToList();
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Cliente não encontrado.");
        client.IsActive = false;
        await db.SaveChangesAsync(cancellationToken);
    }

    private static void Validate(SaveClientDto input)
    {
        ClientDataValidator.Validate(input.Name, input.Email, input.Document);
        if (input.Phone?.Length > 30) throw new QuoteMaster.Domain.Exceptions.DomainException("O telefone deve ter no máximo 30 caracteres.");
        if (input.Address?.Length > 400) throw new QuoteMaster.Domain.Exceptions.DomainException("O endereço deve ter no máximo 400 caracteres.");
    }
    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static ClientDto Map(Client x) => new(x.Id, x.Name, x.Document, x.Email, x.Phone, x.Address, x.CreatedAt, x.IsActive);
}
