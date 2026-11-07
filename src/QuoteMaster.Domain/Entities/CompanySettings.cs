namespace QuoteMaster.Domain.Entities;

public sealed class CompanySettings
{
    public int Id { get; set; } = 1;
    public string CompanyName { get; set; } = "";
    public string? TradeName { get; set; }
    public string? Document { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? LogoPath { get; set; }
    public int DefaultValidityDays { get; set; } = 7;
    public string? DefaultNotes { get; set; }
}
