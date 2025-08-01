using Microsoft.Extensions.DependencyInjection;
using Helsi.TaskManagement.Application.Services.Interfaces;
using Helsi.TaskManagement.Application.Services.Implementations;

namespace Helsi.TaskManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register Application Services
        services.AddScoped<ITaskListService, TaskListService>();
        
        return services;
    }
}