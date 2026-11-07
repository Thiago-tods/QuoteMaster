using System.ComponentModel.DataAnnotations;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Domain.Enums;

namespace QuoteMaster.Web.ViewModels.ProductViewModels;

public sealed class ProductListViewModel
{
    public string? Search { get; init; }
    public string Status { get; init; } = "all";
    public string Sort { get; init; } = "name";
    public bool Descending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }
    public IReadOnlyList<ProductDto> Products { get; init; } = [];
    public int PageCount => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed class ProductFormViewModel
{
    public int? Id { get; set; }
    [Required(ErrorMessage = "Informe o nome do item.")]
    [StringLength(160)]
    [Display(Name = "Nome")]
    public string Name { get; set; } = string.Empty;
    [StringLength(1000)]
    [Display(Name = "Descrição")]
    public string? Description { get; set; }
    [Range(0d, 9999999999.99d, ErrorMessage = "O preço deve ser zero ou maior.")]
    [Display(Name = "Preço padrão")]
    [DataType(DataType.Currency)]
    public decimal DefaultPrice { get; set; }
    [EnumDataType(typeof(ProductType), ErrorMessage = "Selecione um tipo válido.")]
    [Display(Name = "Tipo")]
    public ProductType Type { get; set; } = ProductType.Service;
}
