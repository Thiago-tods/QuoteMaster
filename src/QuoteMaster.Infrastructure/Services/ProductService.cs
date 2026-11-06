using Microsoft.EntityFrameworkCore;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Domain.Entities;
using QuoteMaster.Domain.Exceptions;
using QuoteMaster.Infrastructure.Data;

namespace QuoteMaster.Infrastructure.Services;

public sealed class ProductService(ApplicationDbContext db) : IProductService
{
    public async Task<ProductDto> CreateAsync(SaveProductDto input, CancellationToken cancellationToken = default)
    {
        var product = new Product { Name = string.Empty };
        product.Update(input.Name, input.Description, input.DefaultPrice, input.Type);
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, SaveProductDto input, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Produto não encontrado.");
        product.Update(input.Name, input.Description, input.DefaultPrice, input.Type);
        await db.SaveChangesAsync(cancellationToken);
        return Map(product);
    }

    public async Task<ProductDto?> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return product is null ? null : Map(product);
    }

    public async Task<IReadOnlyList<ProductDto>> SearchAsync(string? search, bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        var query = db.Products.AsNoTracking().AsQueryable();
        if (activeOnly) query = query.Where(x => x.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Name.Contains(term) || (x.Description != null && x.Description.Contains(term)));
        }
        var rows = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return rows.Select(Map).ToList();
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await db.Products.FindAsync([id], cancellationToken) ?? throw new KeyNotFoundException("Produto não encontrado.");
        product.Deactivate();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static ProductDto Map(Product x) => new(x.Id, x.Name, x.Description, x.DefaultPrice, x.Type, x.CreatedAt, x.IsActive);
}
