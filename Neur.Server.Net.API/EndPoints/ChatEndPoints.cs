using System.Security.Claims;
using Neur.Server.Net.API.Contracts.Chats;
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
            .Produces<List<ChatEntity>>(200);

        endpoints.MapPost("/{id}", Delete)
            .WithSummary("Удалить чат")
            .Produces(401)
            .Produces(404)
            .Produces(200);
        
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

    private static async Task<IResult> GetAllUserChats(ClaimsPrincipal user, IChatsRepository repository) {
        var userId = user.FindFirst("userId")?.Value;

        if (userId == null) {
            return Results.BadRequest("Cookies are missing: userId");
        }

        var chats = await repository.GetAllUserChats(Guid.Parse(userId));

        return Results.Ok(chats);
    }

    private static async Task<IResult> Delete(Guid id, IChatsRepository repository) {
        try {
            await repository.Delete(id);
            return Results.Ok(id);
        }

        catch (NullReferenceException) {
            return Results.NotFound("There is no such chat");
        }
    }
}