using QuoteMaster.Application.DTOs;

namespace QuoteMaster.Application.Interfaces;

public interface IClientService
{
    Task<ClientDto> CreateAsync(SaveClientDto input, CancellationToken cancellationToken = default);
    Task<ClientDto> UpdateAsync(int id, SaveClientDto input, CancellationToken cancellationToken = default);
    Task<ClientDto?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClientDto>> SearchAsync(string? search, bool activeOnly = false, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
