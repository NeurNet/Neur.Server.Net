using System.Security.Claims;
using Neur.Server.Net.API.Contracts.GenerationRequests;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.API.EndPoints;

public static class RequestEndPoints {
    public static IEndpointRouteBuilder MapRequestEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/requests")
            .WithTags("Generation Requests")
            .RequireAuthorization("TeacherOrAdmin");

        endpoints.MapPost(String.Empty, GetAll)
            .WithSummary("Получить список всех запросов пользователей");
        
        return endpoints;
    }

    private static async Task<IResult> GetAll(ClaimsPrincipal claims, GenerationRequestService generationRequestService) {
        var user = claims.ToCurrentUser();
        var requests = await generationRequestService.GetAllGenerationRequests(user.userId);
        return Results.Ok(requests.Select(x =>
            new GenerationRequestResponse(x.Id, x.ModelId, x.Model.ModelName, x.TokenCost, x.Status, x.CreatedAt,
                x.StartedAt, x.FinishedAt))
        );
    }
}