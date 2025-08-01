using Helsi.TaskManagement.Api.Infrastructure.Middlewares;

namespace Helsi.TaskManagement.Api.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UseMiddlewares(this IApplicationBuilder app)
    {
        app.UseHttpsRedirection();
        app.UseCors();
        app.UseUserContext();
        app.UseExceptionMiddleware();
    }

    #region Private Methods

    private static void UseUserContext(this IApplicationBuilder app)
    {
        app.UseMiddleware<UserContextMiddleware>();
    }

    private static void UseExceptionMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
    }

    #endregion
}