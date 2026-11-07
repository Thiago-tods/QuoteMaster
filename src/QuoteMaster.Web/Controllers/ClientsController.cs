using Microsoft.AspNetCore.Mvc;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Domain.Exceptions;
using QuoteMaster.Web.ViewModels.ClientViewModels;

namespace QuoteMaster.Web.Controllers;

public sealed class ClientsController(IClientService clients, IQuotationService quotations) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, string status = "all", string sort = "name", bool descending = false, int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var rows = (await clients.SearchAsync(search, cancellationToken: cancellationToken)).AsEnumerable();
        rows = status switch
        {
            "active" => rows.Where(x => x.IsActive),
            "inactive" => rows.Where(x => !x.IsActive),
            _ => rows
        };
        rows = sort.ToLowerInvariant() switch
        {
            "email" => descending ? rows.OrderByDescending(x => x.Email) : rows.OrderBy(x => x.Email),
            "status" => descending ? rows.OrderByDescending(x => x.IsActive) : rows.OrderBy(x => x.IsActive),
            "created" => descending ? rows.OrderByDescending(x => x.CreatedAt) : rows.OrderBy(x => x.CreatedAt),
            _ => descending ? rows.OrderByDescending(x => x.Name) : rows.OrderBy(x => x.Name)
        };
        var ordered = rows.ToArray();
        const int pageSize = 10;
        return View(new ClientListViewModel
        {
            Search = search, Status = status, Sort = sort, Descending = descending, Page = page, PageSize = pageSize,
            TotalCount = ordered.Length, Clients = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToArray()
        });
    }

    [HttpGet]
    public IActionResult Create() => View("Form", new ClientFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View("Form", model);
        try
        {
            var client = await clients.CreateAsync(ToDto(model), cancellationToken);
            TempData["Success"] = $"Cliente {client.Name} cadastrado com sucesso.";
            return RedirectToAction(nameof(Details), new { id = client.Id });
        }
        catch (DomainException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
        return View("Form", model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var client = await clients.GetAsync(id, cancellationToken);
        if (client is null) return NotFound();
        return View("Form", new ClientFormViewModel
        {
            Id = client.Id, Name = client.Name, Document = client.Document, Email = client.Email,
            Phone = client.Phone, Address = client.Address
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();
        if (!ModelState.IsValid) return View("Form", model);
        try
        {
            var client = await clients.UpdateAsync(id, ToDto(model), cancellationToken);
            TempData["Success"] = "Dados do cliente atualizados.";
            return RedirectToAction(nameof(Details), new { id = client.Id });
        }
        catch (DomainException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
        catch (KeyNotFoundException) { return NotFound(); }
        return View("Form", model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var client = await clients.GetAsync(id, cancellationToken);
        if (client is null) return NotFound();
        var clientQuotes = (await quotations.ListAsync(cancellationToken)).Where(x => x.ClientId == id).OrderByDescending(x => x.CreatedAt).ToArray();
        return View(new ClientDetailsViewModel { Client = client, Quotations = clientQuotes });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        try
        {
            await clients.DeactivateAsync(id, cancellationToken);
            TempData["Success"] = "Cliente desativado. O histórico de orçamentos foi preservado.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        return RedirectToAction(nameof(Details), new { id });
    }

    private static SaveClientDto ToDto(ClientFormViewModel model) => new(model.Name, model.Document, model.Email, model.Phone, model.Address);
}
