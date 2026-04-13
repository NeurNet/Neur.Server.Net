using Microsoft.AspNetCore.Mvc;
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
        
        return endpoints;
    }

    private static async Task<IResult> GetAllModels(CancellationToken cancellationToken, [FromServices] IOllamaService service) {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        
        var response = await service.GetOllamaModels(cts.Token);
        return Results.Ok(response);
    }
}