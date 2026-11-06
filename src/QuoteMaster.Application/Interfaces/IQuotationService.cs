using QuoteMaster.Application.DTOs;

namespace QuoteMaster.Application.Interfaces;

public interface IQuotationService
{
    Task<QuotationDto> CreateAsync(CreateQuotationDto input, CancellationToken cancellationToken = default);
    Task<QuotationDto?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<QuotationDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<QuotationDto> SendAsync(int id, CancellationToken cancellationToken = default);
    Task<QuotationDto> ApproveAsync(int id, CancellationToken cancellationToken = default);
    Task<QuotationDto> RejectAsync(int id, CancellationToken cancellationToken = default);
    Task<QuotationDto> ExpireAsync(int id, CancellationToken cancellationToken = default);
}
