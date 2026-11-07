using Microsoft.AspNetCore.Mvc;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Domain.Exceptions;
using QuoteMaster.Web.ViewModels.ProductViewModels;

namespace QuoteMaster.Web.Controllers;

public sealed class ProductsController(IProductService products) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, string status = "all", string sort = "name", bool descending = false, int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var rows = (await products.SearchAsync(search, cancellationToken: cancellationToken)).AsEnumerable();
        rows = status switch
        {
            "active" => rows.Where(x => x.IsActive),
            "inactive" => rows.Where(x => !x.IsActive),
            _ => rows
        };
        rows = sort.ToLowerInvariant() switch
        {
            "price" => descending ? rows.OrderByDescending(x => x.DefaultPrice) : rows.OrderBy(x => x.DefaultPrice),
            "type" => descending ? rows.OrderByDescending(x => x.Type) : rows.OrderBy(x => x.Type),
            _ => descending ? rows.OrderByDescending(x => x.Name) : rows.OrderBy(x => x.Name)
        };
        var ordered = rows.ToArray();
        const int pageSize = 10;
        return View(new ProductListViewModel
        {
            Search = search, Status = status, Sort = sort, Descending = descending, Page = page, PageSize = pageSize,
            TotalCount = ordered.Length, Products = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToArray()
        });
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new ProductFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("Form", model);
        try
        {
            var product = await products.CreateAsync(ToDto(model), cancellationToken);
            TempData["Success"] = $"{product.Name} adicionado ao catálogo.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
        return View("Form", model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var product = await products.GetAsync(id, cancellationToken);
        if (product is null) return NotFound();
        return View("Form", new ProductFormViewModel
        {
            Id = product.Id, Name = product.Name, Description = product.Description,
            DefaultPrice = product.DefaultPrice, Type = product.Type
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", model);
        try
        {
            await products.UpdateAsync(id, ToDto(model), cancellationToken);
            TempData["Success"] = "Item do catálogo atualizado. Orçamentos anteriores mantêm seus valores.";
            return RedirectToAction(nameof(Index));
        }
        catch (DomainException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
        catch (KeyNotFoundException) { return NotFound(); }
        return View("Form", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        try
        {
            await products.DeactivateAsync(id, cancellationToken);
            TempData["Success"] = "Item desativado e removido das opções para novos orçamentos.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Index));
    }

    private static SaveProductDto ToDto(ProductFormViewModel model) => new(model.Name, model.Description, model.DefaultPrice, model.Type);
}
