using QuoteMaster.Domain.Exceptions;
using QuoteMaster.Domain.Rules;

namespace QuoteMaster.UnitTests;

public sealed class QuotationCalculatorTests
{
    [Fact]
    public void CalculatesItemAndQuotationTotals()
    {
        var lines = new[]
        {
            new QuotationLineAmount(2m, 150m, 20m),
            new QuotationLineAmount(3m, 80m, 0m),
            new QuotationLineAmount(1m, 120m, 10m)
        };

        var totals = QuotationCalculator.Calculate(lines, 30m);

        Assert.Equal(630m, totals.Subtotal);
        Assert.Equal(30m, totals.Discount);
        Assert.Equal(600m, totals.Total);
    }

    [Fact]
    public void RoundsHalfCentsAwayFromZero()
    {
        Assert.Equal(1.01m, QuotationCalculator.Round(1.005m));
        Assert.Equal(-1.01m, QuotationCalculator.Round(-1.005m));
    }

    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(-1, 10, 0)]
    [InlineData(1, -1, 0)]
    [InlineData(1, 10, -1)]
    [InlineData(1, 10, 10.01)]
    public void RejectsInvalidLineAmounts(decimal quantity, decimal price, decimal discount)
    {
        Assert.Throws<DomainException>(() => QuotationCalculator.CalculateLineTotal(quantity, price, discount));
    }

    [Fact]
    public void RejectsGeneralDiscountAboveSubtotal()
    {
        Assert.Throws<DomainException>(() => QuotationCalculator.Calculate([new QuotationLineAmount(1, 50, 0)], 50.01m));
    }

    [Fact]
    public void RejectsQuantityWithMorePrecisionThanTheStoredAndDisplayedAmount()
    {
        Assert.Throws<DomainException>(() => QuoteMaster.Domain.Entities.QuotationItem.Create("Cabo", null, 1.0005m, 10m, 0m));
        Assert.Equal(1.001m, QuoteMaster.Domain.Entities.QuotationItem.Create("Cabo", null, 1.001m, 10m, 0m).Quantity);
    }
}
