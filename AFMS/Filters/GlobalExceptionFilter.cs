using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace AFMS.Filters;

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "Unhandled exception occurred.");

        var request = context.HttpContext.Request;
        var acceptsJson = request.Headers.Accept.Any(value =>
            !string.IsNullOrWhiteSpace(value)
            && value.Contains("application/json", StringComparison.OrdinalIgnoreCase));
        var isAjax = string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        if (acceptsJson || isAjax || request.Path.StartsWithSegments("/Home/ProcessAIQuery") || request.Path.StartsWithSegments("/Home/ProcessAddFlightQuery"))
        {
            context.Result = new ObjectResult(new { error = "An unexpected error occurred." })
            {
                StatusCode = 500
            };
        }
        else
        {
            context.Result = new RedirectToActionResult("Error", "Home", null);
        }

        context.ExceptionHandled = true;
    }
}
