using System.ComponentModel.DataAnnotations;

namespace QuoteMaster.Domain.Enums;

public enum ProductType
{
    [Display(Name = "Produto")]
    Product = 1,
    [Display(Name = "Serviço")]
    Service = 2
}
