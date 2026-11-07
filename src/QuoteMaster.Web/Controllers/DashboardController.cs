using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using QuoteMaster.Application.DTOs;
using QuoteMaster.Application.Interfaces;
using QuoteMaster.Domain.Enums;
using QuoteMaster.Web.ViewModels;

namespace QuoteMaster.Web.Controllers;

public sealed class DashboardController(IQuotationService quotations) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var all = await quotations.ListAsync(cancellationToken);
        var approved = all.Where(x => x.Status == QuotationStatus.Approved).ToArray();
        var decisions = approved.Length + all.Count(x => x.Status == QuotationStatus.Rejected);
        var today = DateTime.Now;
        var months = Enumerable.Range(0, 6)
            .Select(offset => new DateTime(today.Year, today.Month, 1).AddMonths(offset - 5))
            .ToArray();

        var model = new DashboardViewModel
        {
            TotalQuotations = all.Count,
            WaitingForResponse = all.Count(x => x.Status == QuotationStatus.Sent),
            ApprovedCount = approved.Length,
            ApprovedValue = approved.Sum(x => x.Total),
            ApprovalRate = decisions == 0 ? 0m : Math.Round(approved.Length * 100m / decisions, 1),
            RecentQuotations = all.OrderByDescending(x => x.CreatedAt).Take(6).ToArray(),
            MonthLabels = months.Select(x => x.ToString("MMM/yy", CultureInfo.GetCultureInfo("pt-BR"))).ToArray(),
            MonthlyCounts = months.Select(x => all.Count(q => q.CreatedAt.ToLocalTime().Year == x.Year && q.CreatedAt.ToLocalTime().Month == x.Month)).ToArray(),
            StatusLabels = ["Rascunho", "Enviado", "Aprovado", "Recusado", "Expirado"],
            StatusCounts = [
                all.Count(x => x.Status == QuotationStatus.Draft),
                all.Count(x => x.Status == QuotationStatus.Sent),
                approved.Length,
                all.Count(x => x.Status == QuotationStatus.Rejected),
                all.Count(x => x.Status == QuotationStatus.Expired)
            ]
        };
        return View(model);
    }
}
