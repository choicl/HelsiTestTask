using Helsi.TaskManagement.Api.Infrastructure.Extensions;
using Helsi.TaskManagement.Application;
using Helsi.TaskManagement.Infrastructure;

namespace Helsi.TaskManagement.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddApplication()
            .AddInfrastructure(configuration)
            .AddSwagger()
            .AddCorsSettings();

        return services;
    }
}