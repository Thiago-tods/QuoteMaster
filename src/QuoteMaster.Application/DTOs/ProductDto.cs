using QuoteMaster.Domain.Enums;

namespace QuoteMaster.Application.DTOs;

public sealed record ProductDto(int Id, string Name, string? Description, decimal DefaultPrice, ProductType Type, DateTime CreatedAt, bool IsActive);

public sealed record SaveProductDto(string Name, string? Description, decimal DefaultPrice, ProductType Type);
