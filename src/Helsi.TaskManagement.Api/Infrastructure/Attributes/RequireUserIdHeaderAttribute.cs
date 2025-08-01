using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Helsi.TaskManagement.Api.Infrastructure.Attributes;

/// <summary>
/// Attribute to require a specific header containing the userId for API requests.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireUserIdHeaderAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _headerName = "X-User-Id";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(_headerName, out var userId) || 
            string.IsNullOrWhiteSpace(userId))
        {
            context.Result = new ObjectResult($"'{_headerName}' header is required")
            {
                StatusCode = StatusCodes.Status401Unauthorized
            };
            return;
        }

        await next();
    }
}