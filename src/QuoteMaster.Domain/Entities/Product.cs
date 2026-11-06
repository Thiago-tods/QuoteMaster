using QuoteMaster.Domain.Enums;
using QuoteMaster.Domain.Exceptions;
using QuoteMaster.Domain.Rules;

namespace QuoteMaster.Domain.Entities;

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal DefaultPrice { get; set; }
    public ProductType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public void Update(string name, string? description, decimal defaultPrice, ProductType type)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("O nome do produto é obrigatório.");
        if (name.Trim().Length > 160) throw new DomainException("O nome do produto deve ter no máximo 160 caracteres.");
        if (defaultPrice < 0) throw new DomainException("O preço não pode ser negativo.");
        if (!Enum.IsDefined(type)) throw new DomainException("O tipo de produto é inválido.");
        if (description?.Length > 1000) throw new DomainException("A descrição deve ter no máximo 1000 caracteres.");
        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        DefaultPrice = QuotationCalculator.Round(defaultPrice);
        Type = type;
    }

    public void Deactivate() => IsActive = false;
}
