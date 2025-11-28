using Microsoft.OpenApi.Models;

namespace KafkaWebApi.Infrastructure.Extensions;

public static class SwaggerExtensions
{
    public static void AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Kafka Web API - Vertical Slice Architecture",
                Version = "v1",
                Description = "API with Kafka integration for message processing using Vertical Slice Architecture"
            });
        });
    }

    public static void UseSwaggerDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}

