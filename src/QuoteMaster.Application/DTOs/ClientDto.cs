namespace QuoteMaster.Application.DTOs;

public sealed record ClientDto(int Id, string Name, string? Document, string? Email, string? Phone, string? Address, DateTime CreatedAt, bool IsActive);

public sealed record SaveClientDto(string Name, string? Document, string? Email, string? Phone, string? Address);
