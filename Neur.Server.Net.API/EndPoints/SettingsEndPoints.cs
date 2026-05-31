using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.Settings;
using Neur.Server.Net.Application.Extensions;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.API.EndPoints;

public static class SettingsEndPoints {
    public static IEndpointRouteBuilder MapSettingsEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/settings")
            .WithTags("Settings")
            .RequireAuthorization("AdminOnly")
            .ProducesProblem(401)
            .ProducesProblem(403);

        endpoints.MapGet(String.Empty, GetSettings)
            .WithSummary("Получить настройки")
            .Produces<List<GetSettingsResponse>>(200);

        endpoints.MapPost("/clients", SetSettings)
            .WithSummary("Обновить настройку клиента")
            .Produces(200)
            .ProducesProblem(400);

        return endpoints;
    }

    private static async Task<IResult> GetSettings(ISettingsService settingsService) {
        var settings = await settingsService.GetSettingsAsync();
        var response = settings.Select(s => new GetSettingsResponse(s.Name, s.BaseUrl, s.Timeout)).ToList();
        return Results.Ok(response);
    }

    private static async Task<IResult> SetSettings(
        [FromQuery] SettingName name,
        [FromBody] SetSettingsRequest request,
        ISettingsService settingsService) {
        var content = SettingsEntityExtensions.CreateContent(name, request.BaseUrl, request.Timeout);
        await settingsService.SetSettingsAsync(new SettingsEntity(name, content));
        return Results.Ok();
    }
}
