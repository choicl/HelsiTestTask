using Helsi.TaskManagement.Api.Infrastructure.Extensions;

namespace Helsi.TaskManagement.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container
        builder.Services.AddControllers();
        builder.Services.AddServices(builder.Configuration);
        
        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        app.UseMiddlewares();
        app.MapControllers();
        app.Run();
    }
}