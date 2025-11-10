using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.Chats;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Application.Services.Contracts.OllamaService;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.API.EndPoints;

public static class ChatEndPoints {
    public static IEndpointRouteBuilder MapChatsEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/chats").WithTags("Chats").RequireAuthorization();

        endpoints.MapPost(String.Empty, Create)
            .WithSummary("Создать новый чат")
            .Produces(401)
            .Produces(404)
            .Produces<CreateChatResponse>(200);

        endpoints.MapGet(String.Empty, GetAllUserChats)
            .WithSummary("Получить список всех чатов авторизованного пользователя")
            .Produces(401)
            .Produces<List<GetChatResponse>>(200);

        endpoints.MapDelete("/{id}", Delete)
            .WithSummary("Удалить чат")
            .Produces(401)
            .Produces(404)
            .Produces(200);

        endpoints.MapGet("/{id}", Get)
            .WithSummary("Получить чат")
            .Produces(401)
            .Produces(404)
            .Produces<GetChatResponse>(200);
        
        endpoints.MapPost("/{id}/generate", Generate)
            .WithSummary("Сгенерировать ответ от нейросети")
            .Produces(401)
            .Produces(404)
            .Produces<string>(200, "text/event-stream");
        
        return endpoints;
    }

    private static async Task<IResult> Create(ClaimsPrincipal user, CreateChatRequest request, IChatsRepository repository, IModelsRepository modelsRepository) {
        var userId = user.FindFirst("userId")?.Value;

        if (userId == null) {
            return Results.BadRequest("Cookies are missing: userId");
        }

        if (await modelsRepository.Get(request.modelId) == null) {
            return Results.NotFound("There is no such chat model");
        }
        
        var chat = ChatEntity.Create(
            id:  Guid.NewGuid(),
            userId: Guid.Parse(userId),
            modelId: request.modelId,
            createdAt:  DateTime.UtcNow,
            updatedAt:  null
        );
        
        await repository.Add(chat);
        
        return Results.Ok(new CreateChatResponse(chat.Id, chat.ModelId));
    }

    private static async Task Generate(Guid id, [FromBody] GenerateRequest request, IChatsRepository repository, 
        OllamaService ollamaService, HttpContext context) {
        var chat = await repository.Get(id);
        if (chat == null) {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Chat not found");
            return;
        }
        if (request.prompt.Length == 0) {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Chat not found");
            return;
        }
        
        // Готовим ответ для SSE
        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/event-stream";
        context.Response.Headers["Cache-Control"] = "no-cache";
        context.Response.Headers["Connection"] = "keep-alive";

        await foreach (var chunk in ollamaService.StreamResponse(
                           new OllamaRequest(chat.Model.ModelName, request.prompt, true)))
        {
            // Формат для SSE: каждая строка = одно событие
            await context.Response.WriteAsync($"data: {chunk}\n\n");
            await context.Response.Body.FlushAsync();
        }

        await context.Response.WriteAsync("event: done\ndata: [DONE]\n\n");
        await context.Response.Body.FlushAsync();
    }

    private static async Task<IResult> GetAllUserChats(ClaimsPrincipal user, IChatsRepository repository) {
        var userId = user.FindFirst("userId")?.Value;

        if (userId == null) {
            return Results.BadRequest("Cookies are missing: userId");
        }

        var chats = await repository.GetAllUserChats(Guid.Parse(userId));
        var result = chats.Select(chat => new GetChatResponse(chat.Id, chat.ModelId, chat.CreatedAt, chat.UpdatedAt)).ToList();
        
        return Results.Ok(chats);
    }

    private static async Task<IResult> Get(Guid id, IChatsRepository repository) {
        var chat = await repository.Get(id);
        if (chat == null) {
            return Results.NotFound("Chat not found");
        }
        return Results.Ok(new GetChatResponse(chat.Id, chat.ModelId, chat.CreatedAt, chat.UpdatedAt));
    }

    private static async Task<IResult> Delete(Guid id, IChatsRepository repository) {
        try {
            await repository.Delete(id);
            return Results.Ok(id);
        }

        catch (NullReferenceException) {
            return Results.NotFound("Chat not found");
        }
    }
}