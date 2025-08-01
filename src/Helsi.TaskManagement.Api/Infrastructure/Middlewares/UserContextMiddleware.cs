namespace Helsi.TaskManagement.Api.Infrastructure.Middlewares;

public class UserContextMiddleware(RequestDelegate next)
{
    private const string UserIdHeader = "X-User-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract userId from X-User-Id header
        if (context.Request.Headers.TryGetValue(UserIdHeader, out var userIdValues))
        {
            var userId = userIdValues.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(userId))
            {
                // Save userId in HttpContext.Items for later use
                context.Items["UserId"] = userId;
            }
        }

        await next(context);
    }
}