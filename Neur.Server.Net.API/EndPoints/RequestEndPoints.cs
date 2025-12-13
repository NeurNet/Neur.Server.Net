using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.API.EndPoints;

public static class RequestEndPoints {
    public static IEndpointRouteBuilder MapRequestEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/requests")
            .WithTags("GenerationRequests")
            .RequireAuthorization();

        endpoints.MapPost(String.Empty, GetAll)
            .WithSummary("Получить список всех запросов");
        
        return endpoints;
    }

    private static async Task<IResult> GetAll(IGenerationRequestsRepository repository) {
        var requests = await repository.GetAll();
        return Results.Ok(requests);
    }
}