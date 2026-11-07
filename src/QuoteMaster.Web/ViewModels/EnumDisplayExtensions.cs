using System.ComponentModel.DataAnnotations;
using System.Reflection;
using QuoteMaster.Domain.Enums;

namespace QuoteMaster.Web.ViewModels;

public static class EnumDisplayExtensions
{
    public static string DisplayName(this QuotationStatus status)
    {
        var field = status.GetType().GetField(status.ToString());
        return field?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? status.ToString();
    }
}
