namespace QuoteMaster.Domain.Entities;

using QuoteMaster.Domain.Rules;

public sealed class QuotationItem
{
    public int Id { get; set; }
    public int QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
    public required string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }

    public static QuotationItem Create(string description, int? productId, decimal quantity, decimal unitPrice, decimal discount)
    {
        if (quantity != Math.Round(quantity, 3, MidpointRounding.AwayFromZero))
            throw new QuoteMaster.Domain.Exceptions.DomainException("A quantidade deve ter no máximo três casas decimais.");
        if (string.IsNullOrWhiteSpace(description))
            throw new QuoteMaster.Domain.Exceptions.DomainException("A descrição do item é obrigatória.");

        if (description.Trim().Length > 300)
            throw new QuoteMaster.Domain.Exceptions.DomainException("A descrição do item deve ter no máximo 300 caracteres.");

        _ = QuotationCalculator.CalculateLineTotal(quantity, unitPrice, discount);
        var roundedPrice = QuotationCalculator.Round(unitPrice);
        var roundedDiscount = QuotationCalculator.Round(discount);
        return new QuotationItem
        {
            Description = description.Trim(),
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = roundedPrice,
            Discount = roundedDiscount,
            Total = QuotationCalculator.CalculateLineTotal(quantity, roundedPrice, roundedDiscount)
        };
    }
}
