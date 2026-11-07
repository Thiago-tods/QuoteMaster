using System.ComponentModel.DataAnnotations;

namespace QuoteMaster.Domain.Enums;

public enum QuotationStatus
{
    [Display(Name = "Rascunho")]
    Draft = 1,
    [Display(Name = "Enviado")]
    Sent = 2,
    [Display(Name = "Aprovado")]
    Approved = 3,
    [Display(Name = "Recusado")]
    Rejected = 4,
    [Display(Name = "Expirado")]
    Expired = 5
}
