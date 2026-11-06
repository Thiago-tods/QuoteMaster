using QuoteMaster.Domain.Exceptions;

namespace QuoteMaster.Domain.Rules;

public readonly record struct QuotationLineAmount(decimal Quantity, decimal UnitPrice, decimal Discount);

public readonly record struct QuotationTotals(decimal Subtotal, decimal Discount, decimal Total);

public static class QuotationCalculator
{
    public static decimal CalculateLineTotal(decimal quantity, decimal unitPrice, decimal discount)
    {
        if (quantity <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");
        if (unitPrice < 0)
            throw new DomainException("O preço unitário não pode ser negativo.");
        if (discount < 0)
            throw new DomainException("O desconto do item não pode ser negativo.");

        var gross = quantity * unitPrice;
        if (discount > gross)
            throw new DomainException("O desconto do item não pode exceder seu valor bruto.");

        return Round(gross - discount);
    }

    public static QuotationTotals Calculate(IEnumerable<QuotationLineAmount> lines, decimal generalDiscount)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (generalDiscount < 0)
            throw new DomainException("O desconto geral não pode ser negativo.");

        var subtotal = Round(lines.Sum(line => CalculateLineTotal(line.Quantity, line.UnitPrice, line.Discount)));
        if (generalDiscount > subtotal)
            throw new DomainException("O desconto geral não pode exceder o subtotal.");

        return new QuotationTotals(subtotal, Round(generalDiscount), Round(subtotal - generalDiscount));
    }

    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
