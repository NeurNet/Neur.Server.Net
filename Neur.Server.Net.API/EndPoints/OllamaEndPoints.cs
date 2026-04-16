using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.Ollama;
using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Application.Interfaces.Services;

namespace Neur.Server.Net.API.EndPoints;

public static class OllamaEndPoints {
    public static IEndpointRouteBuilder MapOllamaEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app
            .MapGroup("/api/ollama")
            .RequireAuthorization("AdminOnly")
            .ProducesProblem(401)
            .WithTags("Ollama");

        endpoints
            .MapGet(String.Empty, GetAllModels)
            .WithSummary("Получить список всех ollama моделей на сервере")
            .Produces<List<OllamaModel>>(200);

        endpoints
            .MapPost("pull", LoadModel)
            .WithSummary("Загрузить модель Ollama на сервер")
            .Produces<OllamaLoadModelResponse>(200, "application/x-ndjson");
        
        return endpoints;
    }

    private static async Task<IResult> GetAllModels(CancellationToken cancellationToken, [FromServices] IOllamaService service) {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        
        var response = await service.GetOllamaModelsAsync(cts.Token);
        return Results.Ok(response);
    }

    private static async Task LoadModel(LoadModelRequest request, [FromServices] IOllamaService service, 
        HttpContext context, CancellationToken cancellationToken) {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30));
        
        context.Response.ContentType = "application/x-ndjson";
        context.Response.Headers["Cache-Control"] = "no-cache";
        context.Response.Headers["Connection"] = "keep-alive";

        try {
            await foreach (var chunk in service.LoadModelAsync(request.name, cts.Token)) {
                var json = JsonSerializer.Serialize(chunk);
                await context.Response.WriteAsync(json + "\n", cts.Token);
                await context.Response.Body.FlushAsync(cts.Token);
            }
        }
        catch (OperationCanceledException) {
            if (!context.Response.HasStarted) {
                context.Response.StatusCode = 408;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Request timeout", status = 408 }));
            }
        }
        catch (Exception) {
            if (!context.Response.HasStarted) {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "Internal server error", status = 500 }));
            }
            throw;
        }
        finally {
            await context.Response.CompleteAsync();
        }
    }
}