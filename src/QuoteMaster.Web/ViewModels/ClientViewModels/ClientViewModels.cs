using System.ComponentModel.DataAnnotations;
using QuoteMaster.Application.DTOs;

namespace QuoteMaster.Web.ViewModels.ClientViewModels;

public sealed class ClientListViewModel
{
    public string? Search { get; init; }
    public string Status { get; init; } = "all";
    public string Sort { get; init; } = "name";
    public bool Descending { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }
    public IReadOnlyList<ClientDto> Clients { get; init; } = [];
    public int PageCount => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

public sealed class ClientFormViewModel
{
    public int? Id { get; set; }
    [Required(ErrorMessage = "Informe o nome do cliente.")]
    [StringLength(160, ErrorMessage = "Use até 160 caracteres.")]
    [Display(Name = "Nome completo ou razão social")]
    public string Name { get; set; } = string.Empty;
    [StringLength(18, ErrorMessage = "Use até 18 caracteres.")]
    [Display(Name = "CPF ou CNPJ")]
    public string? Document { get; set; }
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(254)]
    [Display(Name = "E-mail")]
    public string? Email { get; set; }
    [StringLength(30)]
    [Display(Name = "Telefone")]
    public string? Phone { get; set; }
    [StringLength(400)]
    [Display(Name = "Endereço")]
    public string? Address { get; set; }
}

public sealed class ClientDetailsViewModel
{
    public required ClientDto Client { get; init; }
    public IReadOnlyList<QuotationDto> Quotations { get; init; } = [];
}
