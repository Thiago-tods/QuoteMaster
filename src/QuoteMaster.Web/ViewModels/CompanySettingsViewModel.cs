using System.ComponentModel.DataAnnotations;

namespace QuoteMaster.Web.ViewModels;

public sealed class CompanySettingsViewModel
{
    [Required, StringLength(160)] [Display(Name = "Nome da empresa")]
    public string CompanyName { get; set; } = "Minha empresa";
    [StringLength(160)] [Display(Name = "Nome fantasia")] public string? TradeName { get; set; }
    [StringLength(18)] [Display(Name = "CNPJ/CPF")] public string? Document { get; set; }
    [EmailAddress, StringLength(254)] [Display(Name = "E-mail")] public string? Email { get; set; }
    [StringLength(30)] [Display(Name = "Telefone")] public string? Phone { get; set; }
    [StringLength(400)] [Display(Name = "Endereço")] public string? Address { get; set; }
    [Range(1, 365)] [Display(Name = "Validade padrão (dias)")] public int DefaultValidityDays { get; set; } = 7;
    [StringLength(2000)] [Display(Name = "Observações padrão")] public string? DefaultNotes { get; set; }
    public string? LogoPath { get; set; }
    public IFormFile? Logo { get; set; }
}
