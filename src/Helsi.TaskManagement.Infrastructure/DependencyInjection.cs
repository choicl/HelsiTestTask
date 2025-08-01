using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Helsi.TaskManagement.Domain.Interfaces;
using Helsi.TaskManagement.Infrastructure.Common;
using Helsi.TaskManagement.Infrastructure.Persistence.Context;
using Helsi.TaskManagement.Infrastructure.Persistence.Repositories;

namespace Helsi.TaskManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // MongoDB Configuration
        var mongoSettings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()
            ?? throw new InvalidOperationException("MongoDbSettings configuration is missing");

        services.AddSingleton(mongoSettings);
        services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoSettings.ConnectionString));
        
        services.AddScoped<IMongoDatabase>(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));
        services.AddScoped<MongoDbContext>();

        // Repository Registration
        services.AddScoped<ITaskListRepository, TaskListRepository>();
        
        return services;
    }
}