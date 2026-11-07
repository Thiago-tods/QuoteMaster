using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace QuoteMaster.Web.Controllers;

public sealed class ErrorController : Controller
{
    [Route("/Error")]
    public IActionResult Index()
    {
        var error = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        ViewData["OriginalPath"] = error?.Path;
        return View("~/Views/Shared/Error.cshtml");
    }
}
