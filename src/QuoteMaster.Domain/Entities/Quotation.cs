using QuoteMaster.Domain.Enums;
using QuoteMaster.Domain.Exceptions;

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
    public QuotationStatus Status { get; private set; } = QuotationStatus.Draft;
    public string? Notes { get; set; }
    public ICollection<QuotationItem> Items { get; set; } = new List<QuotationItem>();

    public void MarkSent()
    {
        EnsureStatus(QuotationStatus.Draft, "Somente um rascunho pode ser enviado.");
        Status = QuotationStatus.Sent;
    }

    public void Approve()
    {
        EnsureStatus(QuotationStatus.Sent, "Somente um orçamento enviado pode ser aprovado.");
        Status = QuotationStatus.Approved;
    }

    public void Reject()
    {
        EnsureStatus(QuotationStatus.Sent, "Somente um orçamento enviado pode ser rejeitado.");
        Status = QuotationStatus.Rejected;
    }

    public void Expire(DateTime utcNow)
    {
        EnsureStatus(QuotationStatus.Sent, "Somente um orçamento enviado pode expirar.");
        if (utcNow.Date <= ValidUntil.Date)
            throw new DomainException("O orçamento ainda está dentro do prazo de validade.");
        Status = QuotationStatus.Expired;
    }

    public void EnsureEditable()
    {
        if (Status != QuotationStatus.Draft)
            throw new DomainException("Somente rascunhos podem ser alterados.");
    }

    private void EnsureStatus(QuotationStatus expected, string message)
    {
        if (Status != expected) throw new DomainException(message);
    }
}
