using QuoteMaster.Application.DTOs;

namespace QuoteMaster.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateAsync(SaveProductDto input, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(int id, SaveProductDto input, CancellationToken cancellationToken = default);
    Task<ProductDto?> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductDto>> SearchAsync(string? search, bool activeOnly = false, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
