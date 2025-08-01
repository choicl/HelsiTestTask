using System.Reflection;
using Helsi.TaskManagement.Api.Infrastructure.Filters;

namespace Helsi.TaskManagement.Api.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() {
                Title = "Task Management API",
                Version = "v1",
                Description = "RESTful API for managing task lists with user sharing capabilities"
            });
            
            // Include comments for documentation
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Add X-User-Id header parameter to all operations
            c.OperationFilter<UserIdHeaderOperationFilter>();
        });

        return services;
    }

    public static IServiceCollection AddCorsSettings(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });
        
        return services;
    }
}