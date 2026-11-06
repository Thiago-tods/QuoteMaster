using Microsoft.EntityFrameworkCore;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Domain.Entities;
using QuoteMaster.Domain.Exceptions;
using QuoteMaster.Domain.Rules;
using QuoteMaster.Infrastructure.Data;

namespace QuoteMaster.Infrastructure.Services;

public sealed class QuotationService(
    ApplicationDbContext db,
    IQuotationNumberGenerator numberGenerator,
    TimeProvider timeProvider) : IQuotationService
{
    public async Task<QuotationDto> CreateAsync(CreateQuotationDto input, CancellationToken cancellationToken = default)
    {
        var client = await db.Clients.SingleOrDefaultAsync(x => x.Id == input.ClientId, cancellationToken)
            ?? throw new KeyNotFoundException("Cliente não encontrado.");
        if (!client.IsActive) throw new DomainException("Não é possível criar um orçamento para um cliente inativo.");
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (input.ValidUntil.Date < now.Date) throw new DomainException("A validade do orçamento não pode estar no passado.");
        if (input.Items is null || input.Items.Count == 0) throw new DomainException("O orçamento precisa conter ao menos um item.");
        if (input.Notes?.Length > 2000) throw new DomainException("As observações devem ter no máximo 2000 caracteres.");

        var productIds = input.Items.Where(x => x.ProductId.HasValue).Select(x => x.ProductId!.Value).Distinct().ToArray();
        var products = await db.Products.Where(x => productIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);
        var items = new List<QuotationItem>(input.Items.Count);
        foreach (var item in input.Items)
        {
            if (item.ProductId is int productId)
            {
                if (!products.TryGetValue(productId, out var product)) throw new KeyNotFoundException($"Produto {productId} não encontrado.");
                if (!product.IsActive) throw new DomainException($"O produto '{product.Name}' está inativo.");
                items.Add(QuotationItem.Create(product.Name, product.Id, item.Quantity, product.DefaultPrice, item.Discount));
            }
            else
            {
                if (item.UnitPrice is null) throw new DomainException("Informe o preço unitário dos itens personalizados.");
                items.Add(QuotationItem.Create(item.Description ?? string.Empty, null, item.Quantity, item.UnitPrice.Value, item.Discount));
            }
        }

        var totals = QuotationCalculator.Calculate(items.Select(x => new QuotationLineAmount(x.Quantity, x.UnitPrice, x.Discount)), input.Discount);
        var quotation = new Quotation
        {
            Number = await numberGenerator.GenerateAsync(now, cancellationToken),
            ClientId = client.Id,
            Client = client,
            CreatedAt = now,
            ValidUntil = input.ValidUntil,
            Subtotal = totals.Subtotal,
            Discount = totals.Discount,
            Total = totals.Total,
            Notes = string.IsNullOrWhiteSpace(input.Notes) ? null : input.Notes.Trim(),
            Items = items
        };
        db.Quotations.Add(quotation);
        await db.SaveChangesAsync(cancellationToken);
        return Map(quotation);
    }

    public async Task<QuotationDto?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var quotation = await Query().AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return quotation is null ? null : Map(quotation);
    }

    public async Task<IReadOnlyList<QuotationDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var quotations = await Query().AsNoTracking().OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
        return quotations.Select(Map).ToList();
    }

    public Task<QuotationDto> SendAsync(int id, CancellationToken cancellationToken = default) => TransitionAsync(id, x => x.MarkSent(), cancellationToken);
    public Task<QuotationDto> ApproveAsync(int id, CancellationToken cancellationToken = default) => TransitionAsync(id, x => x.Approve(), cancellationToken);
    public Task<QuotationDto> RejectAsync(int id, CancellationToken cancellationToken = default) => TransitionAsync(id, x => x.Reject(), cancellationToken);
    public Task<QuotationDto> ExpireAsync(int id, CancellationToken cancellationToken = default) => TransitionAsync(id, x => x.Expire(timeProvider.GetUtcNow().UtcDateTime), cancellationToken);

    private async Task<QuotationDto> TransitionAsync(int id, Action<Quotation> transition, CancellationToken cancellationToken)
    {
        var quotation = await Query().SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException("Orçamento não encontrado.");
        transition(quotation);
        await db.SaveChangesAsync(cancellationToken);
        return Map(quotation);
    }

    private IQueryable<Quotation> Query() => db.Quotations.Include(x => x.Client).Include(x => x.Items);

    private static QuotationDto Map(Quotation x) => new(
        x.Id, x.Number, x.ClientId, x.Client.Name, x.CreatedAt, x.ValidUntil,
        x.Subtotal, x.Discount, x.Total, x.Status, x.Notes,
        x.Items.Select(item => new QuotationItemDto(item.Id, item.ProductId, item.Description, item.Quantity, item.UnitPrice, item.Discount, item.Total)).ToList());
}
