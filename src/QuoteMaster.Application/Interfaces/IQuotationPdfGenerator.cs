using QuoteMaster.Application.DTOs;

namespace QuoteMaster.Application.Interfaces;

public interface IQuotationPdfGenerator
{
    Task<byte[]> GenerateAsync(QuotationDto quotation, CompanySettingsDto company, CompanyLogoDto? logo, CancellationToken cancellationToken = default);
}
