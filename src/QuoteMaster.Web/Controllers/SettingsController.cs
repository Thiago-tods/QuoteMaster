using Microsoft.AspNetCore.Mvc;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Web.ViewModels;

namespace QuoteMaster.Web.Controllers;

public sealed class SettingsController(ICompanySettingsService settings) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var x = await settings.GetAsync(ct);
        return View(new CompanySettingsViewModel { CompanyName = x.CompanyName, TradeName = x.TradeName, Document = x.Document, Email = x.Email, Phone = x.Phone, Address = x.Address, DefaultValidityDays = x.DefaultValidityDays, DefaultNotes = x.DefaultNotes, LogoPath = x.LogoPath });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CompanySettingsViewModel model, CancellationToken ct)
    {
        if (model.Logo is { Length: > 2_097_152 }) ModelState.AddModelError(nameof(model.Logo), "A imagem deve ter até 2 MiB.");
        if (!ModelState.IsValid) return View(model);
        try
        {
            await settings.UpdateAsync(new UpdateCompanySettingsDto(model.CompanyName, model.TradeName, model.Document, model.Email, model.Phone, model.Address, model.DefaultValidityDays, model.DefaultNotes), ct);
            if (model.Logo is { Length: > 0 })
            {
                await using var stream = new MemoryStream(); await model.Logo.CopyToAsync(stream, ct);
                await settings.SaveLogoAsync(stream.ToArray(), ct);
            }
            TempData["Success"] = "Configurações salvas.";
            return RedirectToAction(nameof(Index));
        }
        catch (ArgumentException ex) { ModelState.AddModelError(nameof(model.Logo), ex.Message); return View(model); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLogo(CancellationToken ct) { await settings.RemoveLogoAsync(ct); TempData["Success"] = "Logotipo removido."; return RedirectToAction(nameof(Index)); }

    [HttpGet]
    public async Task<IActionResult> Logo(CancellationToken ct)
    {
        var logo = await settings.GetLogoAsync(ct);
        return logo is null ? NotFound() : File(logo.Content, logo.ContentType);
    }
}
