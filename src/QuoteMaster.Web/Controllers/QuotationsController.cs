using Microsoft.AspNetCore.Mvc;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Domain.Enums;
using QuoteMaster.Domain.Exceptions;
using QuoteMaster.Web.ViewModels.QuotationViewModels;

namespace QuoteMaster.Web.Controllers;

public sealed class QuotationsController(IQuotationService quotations, IClientService clients, IProductService products) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? search, QuotationStatus? status, DateTime? from, DateTime? to, string sort = "date", bool descending = true, int page = 1, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        var rows = (await quotations.ListAsync(cancellationToken)).AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            rows = rows.Where(x => x.Number.Contains(term, StringComparison.OrdinalIgnoreCase) || x.ClientName.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        if (status.HasValue) rows = rows.Where(x => x.Status == status.Value);
        if (from.HasValue) rows = rows.Where(x => x.CreatedAt.Date >= from.Value.Date);
        if (to.HasValue) rows = rows.Where(x => x.CreatedAt.Date <= to.Value.Date);
        rows = sort.ToLowerInvariant() switch
        {
            "number" => descending ? rows.OrderByDescending(x => x.Number) : rows.OrderBy(x => x.Number),
            "client" => descending ? rows.OrderByDescending(x => x.ClientName) : rows.OrderBy(x => x.ClientName),
            "total" => descending ? rows.OrderByDescending(x => x.Total) : rows.OrderBy(x => x.Total),
            "status" => descending ? rows.OrderByDescending(x => x.Status) : rows.OrderBy(x => x.Status),
            _ => descending ? rows.OrderByDescending(x => x.CreatedAt) : rows.OrderBy(x => x.CreatedAt)
        };
        var ordered = rows.ToArray();
        const int pageSize = 10;
        return View(new QuotationListViewModel
        {
            Search = search, Status = status, From = from, To = to, Sort = sort, Descending = descending,
            Page = page, PageSize = pageSize, TotalCount = ordered.Length,
            Quotations = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToArray()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? clientId, CancellationToken cancellationToken)
    {
        var model = new QuotationCreateViewModel { ClientId = clientId ?? 0 };
        await PopulateChoicesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuotationCreateViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateChoicesAsync(model, cancellationToken);
            return View(model);
        }

        try
        {
            var created = await quotations.CreateAsync(new CreateQuotationDto(
                model.ClientId, model.ValidUntil, model.Discount, model.Notes,
                model.Items.Select(x => new CreateQuotationItemDto(x.ProductId, x.Description, x.Quantity, x.UnitPrice, x.Discount)).ToArray()), cancellationToken);
            TempData["Success"] = $"Orçamento {created.Number} salvo como rascunho.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (DomainException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
        catch (KeyNotFoundException ex) { ModelState.AddModelError(string.Empty, ex.Message); }

        await PopulateChoicesAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var quotation = await quotations.GetAsync(id, cancellationToken);
        return quotation is null ? NotFound() : View(quotation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Send(int id, CancellationToken cancellationToken) => TransitionAsync(id, quotations.SendAsync, "Orçamento marcado como enviado.", cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Approve(int id, CancellationToken cancellationToken) => TransitionAsync(id, quotations.ApproveAsync, "Orçamento aprovado.", cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Reject(int id, CancellationToken cancellationToken) => TransitionAsync(id, quotations.RejectAsync, "Orçamento recusado.", cancellationToken);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Expire(int id, CancellationToken cancellationToken) => TransitionAsync(id, quotations.ExpireAsync, "Orçamento marcado como expirado.", cancellationToken);

    private async Task<IActionResult> TransitionAsync(int id, Func<int, CancellationToken, Task<QuotationDto>> action, string message, CancellationToken cancellationToken)
    {
        try
        {
            await action(id, cancellationToken);
            TempData["Success"] = message;
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (DomainException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private async Task PopulateChoicesAsync(QuotationCreateViewModel model, CancellationToken cancellationToken)
    {
        model.Clients = await clients.SearchAsync(null, activeOnly: true, cancellationToken);
        model.Products = await products.SearchAsync(null, activeOnly: true, cancellationToken);
        if (model.Items.Count == 0) model.Items.Add(new QuotationItemInputViewModel());
    }
}
