using QuoteMaster.Domain.Entities;
using QuoteMaster.Domain.Enums;
using QuoteMaster.Domain.Exceptions;

namespace QuoteMaster.UnitTests;

public sealed class QuotationStatusTests
{
    private static Quotation NewQuotation() => new()
    {
        Number = "ORC-2026-000001",
        ValidUntil = new DateTime(2026, 11, 30)
    };

    [Fact]
    public void SupportsDraftSentApprovedFlow()
    {
        var quotation = NewQuotation();
        quotation.MarkSent();
        quotation.Approve();
        Assert.Equal(QuotationStatus.Approved, quotation.Status);
    }

    [Fact]
    public void SupportsSentRejectedAndExpiredFlows()
    {
        var rejected = NewQuotation();
        rejected.MarkSent();
        rejected.Reject();
        Assert.Equal(QuotationStatus.Rejected, rejected.Status);

        var expired = NewQuotation();
        expired.MarkSent();
        expired.Expire(new DateTime(2026, 12, 1));
        Assert.Equal(QuotationStatus.Expired, expired.Status);
    }

    [Fact]
    public void DoesNotAllowTransitionsBackToDraftOrBetweenFinalStates()
    {
        var quotation = NewQuotation();
        quotation.MarkSent();
        quotation.Approve();
        Assert.Throws<DomainException>(() => quotation.MarkSent());
        Assert.Throws<DomainException>(() => quotation.Reject());
    }

    [Fact]
    public void DoesNotExpireBeforeValidityEnds()
    {
        var quotation = NewQuotation();
        quotation.MarkSent();
        Assert.Throws<DomainException>(() => quotation.Expire(new DateTime(2026, 11, 30)));
    }
}
