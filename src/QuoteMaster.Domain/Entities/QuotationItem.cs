namespace QuoteMaster.Domain.Entities;

public sealed class QuotationItem
{
    public int Id { get; set; }
    public int QuotationId { get; set; }
    public Quotation Quotation { get; set; } = null!;
    public int? ProductId { get; set; }
    public required string Description { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
}
