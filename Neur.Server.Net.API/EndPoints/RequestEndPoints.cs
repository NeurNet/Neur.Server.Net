using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.GenerationRequests;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.Application.Exceptions;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.API.EndPoints;

public static class RequestEndPoints {
    public static IEndpointRouteBuilder MapRequestEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/requests")
            .WithTags("Generation Requests")
            .RequireAuthorization("TeacherOrAdmin")
            .ProducesProblem(401);
        
        endpoints.MapGet(String.Empty, GetPartByRoleAsync)
            .WithSummary("Получить отсортированную часть запросов пользователей")
            .Produces<GenerationRequestResponse>(200)
            .Produces(400);
        
        return endpoints;
    }

    private static async Task<IResult> GetPartByRoleAsync(GenerationRequestService service, ClaimsPrincipal claims, int page = 1, int pageSize = 20) {
        if (page < 1 || pageSize < 1) {
            throw new ValidateException("Query parameters must be greater than zero");
        }
        
        var userInfo = claims.ToCurrentUser();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(30));
        var requests = await service.GetPartAsync(userInfo.userId, page, pageSize, cts.Token);
        return Results.Ok(requests.ToResponse());
    }
}