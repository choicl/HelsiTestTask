namespace Helsi.TaskManagement.Api.Infrastructure.Extensions;

public static class HttpContextExtensions
{
    public static string? GetUserId(this HttpContext context)
    {
        return context.Items["UserId"] as string;
    }
}