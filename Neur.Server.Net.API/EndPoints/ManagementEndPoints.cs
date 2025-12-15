using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.Management;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Services;

namespace Neur.Server.Net.API.EndPoints;

public static class ManagementEndPoints {
    public static IEndpointRouteBuilder MapManagementEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/management")
            .WithTags("Management")
            .RequireAuthorization("TeacherOrAdmin");

        endpoints.MapPost("/user/tokens", GiveTokens)
            .WithSummary("Передать токены пользователю")
            .Produces(200)
            .Produces(400)
            .Produces(401)
            .Produces(404);
        
        return endpoints;
    }

    private static async Task<IResult> GiveTokens(ClaimsPrincipal claims, [FromBody] GiveTokensRequest  request, ITokenService tokenService) {
        var user =  claims.ToCurrentUser();
        try {
            await tokenService.GiveTokens(user.userId, request.user_id, request.token_count);
        }
        catch (NotFoundException ex) {
            return Results.NotFound(ex.Message);
        }
        catch (BillingException ex) {
            return Results.BadRequest(ex.Message);
        }
        return Results.Ok();
    }
}