namespace QuoteMaster.Application.Interfaces;

public interface IQuotationNumberGenerator
{
    Task<string> GenerateAsync(DateTime quotationLocalDate, CancellationToken cancellationToken = default);
}
