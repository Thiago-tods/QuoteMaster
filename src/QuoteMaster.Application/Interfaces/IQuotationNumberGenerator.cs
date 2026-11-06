namespace QuoteMaster.Application.Interfaces;

public interface IQuotationNumberGenerator
{
    Task<string> GenerateAsync(DateTime createdAtUtc, CancellationToken cancellationToken = default);
}
