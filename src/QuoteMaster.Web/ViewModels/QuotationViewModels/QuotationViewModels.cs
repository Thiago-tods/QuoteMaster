using System.ComponentModel.DataAnnotations;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Domain.Enums;

namespace QuoteMaster.Web.ViewModels.QuotationViewModels;

public sealed class QuotationListViewModel
{
    public string? Search { get; init; }
    public QuotationStatus? Status { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public string Sort { get; init; } = "date";
    public bool Descending { get; init; } = true;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }
    public IReadOnlyList<QuotationDto> Quotations { get; init; } = [];
    public int PageCount => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed class QuotationCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um cliente.")]
    [Display(Name = "Cliente")]
    public int ClientId { get; set; }
    [Required(ErrorMessage = "Informe a data de validade.")]
    [DataType(DataType.Date)]
    [Display(Name = "Válido até")]
    public DateTime ValidUntil { get; set; } = DateTime.Today.AddDays(7);
    [Range(0d, 999999999999.99d, ErrorMessage = "O desconto deve ser zero ou maior.")]
    [Display(Name = "Desconto geral")]
    public decimal Discount { get; set; }
    [StringLength(2000)]
    [Display(Name = "Observações e condições")]
    public string? Notes { get; set; }
    [MinLength(1, ErrorMessage = "Adicione ao menos um item ao orçamento.")]
    public List<QuotationItemInputViewModel> Items { get; set; } = [new()];
    public IReadOnlyList<ClientDto> Clients { get; set; } = [];
    public IReadOnlyList<ProductDto> Products { get; set; } = [];
}

public sealed class QuotationItemInputViewModel
{
    [Display(Name = "Produto ou serviço")]
    public int? ProductId { get; set; }
    [StringLength(300)]
    [Display(Name = "Descrição do item personalizado")]
    public string? Description { get; set; }
    [Range(0.001d, 999999999.999d, ErrorMessage = "A quantidade deve ser maior que zero.")]
    [Display(Name = "Quantidade")]
    public decimal Quantity { get; set; } = 1m;
    [Range(0d, 9999999999.99d, ErrorMessage = "O valor deve ser zero ou maior.")]
    [Display(Name = "Valor unitário")]
    public decimal? UnitPrice { get; set; }
    [Range(0d, 9999999999.99d, ErrorMessage = "O desconto deve ser zero ou maior.")]
    [Display(Name = "Desconto")]
    public decimal Discount { get; set; }
}
