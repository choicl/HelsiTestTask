using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Helsi.TaskManagement.Api.Infrastructure.Filters;

/// <summary>
/// Filter to add a custom header for user identification in Swagger documentation.
/// </summary>
public class UserIdHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-User-Id",
            In = ParameterLocation.Header,
            Required = false, // Set to false to check the userId exceptions in swagger. Should be set to true
            Description = "User identifier for authentication and authorization",
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}