namespace QuoteMaster.Domain.Entities;

public sealed class Client
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Document { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}
