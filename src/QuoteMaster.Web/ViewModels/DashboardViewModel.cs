using QuoteMaster.Application.DTOs;

namespace QuoteMaster.Web.ViewModels;

public sealed class DashboardViewModel
{
    public int TotalQuotations { get; init; }
    public int WaitingForResponse { get; init; }
    public int ApprovedCount { get; init; }
    public decimal ApprovedValue { get; init; }
    public decimal ApprovalRate { get; init; }
    public IReadOnlyList<QuotationDto> RecentQuotations { get; init; } = [];
    public IReadOnlyList<string> MonthLabels { get; init; } = [];
    public IReadOnlyList<int> MonthlyCounts { get; init; } = [];
    public IReadOnlyList<string> StatusLabels { get; init; } = [];
    public IReadOnlyList<int> StatusCounts { get; init; } = [];
}
