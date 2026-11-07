namespace QuoteMaster.Application.DTOs;

public sealed record CompanySettingsDto(int Id, string CompanyName, string? TradeName, string? Document,
    string? Email, string? Phone, string? Address, string? LogoPath, int DefaultValidityDays, string? DefaultNotes);

public sealed record UpdateCompanySettingsDto(string CompanyName, string? TradeName, string? Document,
    string? Email, string? Phone, string? Address, int DefaultValidityDays, string? DefaultNotes);

public sealed record CompanyLogoDto(byte[] Content, string ContentType);
