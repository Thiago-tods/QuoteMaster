using QuoteMaster.Domain.Enums;

namespace QuoteMaster.Application.DTOs;

public sealed record CreateQuotationItemDto(int? ProductId, string? Description, decimal Quantity, decimal? UnitPrice, decimal Discount);

public sealed record CreateQuotationDto(int ClientId, DateTime ValidUntil, decimal Discount, string? Notes, IReadOnlyCollection<CreateQuotationItemDto> Items);

public sealed record QuotationItemDto(int Id, int? ProductId, string Description, decimal Quantity, decimal UnitPrice, decimal Discount, decimal Total);

public sealed record QuotationDto(int Id, string Number, int ClientId, string ClientName, DateTime CreatedAt, DateTime ValidUntil, decimal Subtotal, decimal Discount, decimal Total, QuotationStatus Status, string? Notes, IReadOnlyCollection<QuotationItemDto> Items);
