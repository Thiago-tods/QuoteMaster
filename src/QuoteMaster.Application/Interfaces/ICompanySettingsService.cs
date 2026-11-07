using QuoteMaster.Application.DTOs;

namespace QuoteMaster.Application.Interfaces;

public interface ICompanySettingsService
{
    Task<CompanySettingsDto> GetAsync(CancellationToken cancellationToken = default);
    Task<CompanySettingsDto> UpdateAsync(UpdateCompanySettingsDto input, CancellationToken cancellationToken = default);
    Task<CompanySettingsDto> SaveLogoAsync(byte[] content, CancellationToken cancellationToken = default);
    Task<CompanySettingsDto> RemoveLogoAsync(CancellationToken cancellationToken = default);
    Task<CompanyLogoDto?> GetLogoAsync(CancellationToken cancellationToken = default);
}
