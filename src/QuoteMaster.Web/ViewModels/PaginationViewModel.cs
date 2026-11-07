namespace QuoteMaster.Web.ViewModels;

public sealed record PaginationViewModel(int Page, int PageCount, int TotalCount, int PageSize);
