using QuoteMaster.Domain.Enums;

namespace QuoteMaster.Domain.Entities;

public sealed class Quotation
{
    public int Id { get; set; }
    public required string Number { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ValidUntil { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public QuotationStatus Status { get; set; } = QuotationStatus.Draft;
    public string? Notes { get; set; }
    public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();
}
