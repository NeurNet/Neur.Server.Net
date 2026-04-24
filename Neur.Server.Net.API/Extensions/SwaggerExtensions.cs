using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;
using Neur.Server.Net.API.Contracts;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Neur.Server.Net.API.Extensions;

class ErrorResponseFilter : IOperationFilter {
    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
        var errorSchema = context.SchemaGenerator.GenerateSchema(typeof(ServerErrorResponse), context.SchemaRepository);
        var errorCodes = operation.Responses.Keys
            .Where(k => int.TryParse(k, out var code) && code >= 400);

        foreach (var code in errorCodes) {
            operation.Responses[code] = new OpenApiResponse {
                Description = operation.Responses[code].Description,
                Content = new Dictionary<string, OpenApiMediaType> {
                    ["application/json"] = new() { Schema = errorSchema }
                }
            };
        }
    }
}

public static class SwaggerExtensions {
    public static void AddSwaggerApi(this IServiceCollection services) {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options => {
            options.SwaggerDoc("v1", new OpenApiInfo {
                Title = "Neur Server API",
                Version = "v1"
            });
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.OperationFilter<ErrorResponseFilter>();
        });
    }
}