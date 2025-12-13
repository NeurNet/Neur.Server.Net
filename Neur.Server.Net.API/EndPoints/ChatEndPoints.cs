using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.Chats;
using Neur.Server.Net.API.Contracts.Messages;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.Application.Clients;
using Neur.Server.Net.Application.Data;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Application.Services.Background;
using Neur.Server.Net.Application.Services.Contracts.OllamaService;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Exeptions;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.API.EndPoints;

public static class ChatEndPoints {
    public static IEndpointRouteBuilder MapChatsEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/chats")
            .WithTags("Chats")
            .RequireAuthorization()
            .ProducesProblem(401);

        endpoints.MapPost(String.Empty, Create)
            .WithSummary("Создать новый чат")
            .Produces(401)
            .Produces(404)
            .Produces<CreateChatResponse>(200);

        endpoints.MapGet(String.Empty, GetAllUserChats)
            .WithSummary("Получить список всех чатов пользователя")
            .Produces(401)
            .Produces<List<GetChatResponse>>(200);

        endpoints.MapGet("/{id}", Get)
            .WithSummary("Получить чат")
            .Produces(401)
            .Produces(404)
            .Produces<GetFullChatResponse>(200);
        
        endpoints.MapPost("/{id}/generate", Generate)
            .WithSummary("Сгенерировать ответ от нейросети")
            .Produces(401)
            .Produces(404)
            .Produces<string>(200, "text/event-stream");
        
        endpoints.MapDelete("/{id}", Delete)
            .WithSummary("Удалить чат")
            .Produces(401)
            .Produces(404)
            .Produces(200);
        
        return endpoints;
    }

    private static async Task<IResult> Create(ClaimsPrincipal claimsPrincipal, CreateChatRequest request, ChatService chatService) {
        try {
            var user = claimsPrincipal.ToCurrentUser();
            var chat = await chatService.CreateChatAsync(user.userId, request.modelId);

            return Results.Ok(new CreateChatResponse(chat.Id, chat.ModelId));
        }
        catch (CreatingEntityException ex) {
            return Results.BadRequest("Error creating chat");
        }
        catch (Exception) {
            return Results.InternalServerError();
        }
    }

    private static async Task Generate(Guid id, [FromBody] GenerateRequest request, GenerationQueueService generationQueueService, IChatsRepository repository, 
        ChatService chatService, ClaimsPrincipal claimsPrincipal, HttpContext context) {
        
        var user = claimsPrincipal.ToCurrentUser();
        
        // Готовим ответ для SSE
        context.Response.ContentType = "text/event-stream";
        context.Response.Headers["Cache-Control"] = "no-cache";
        context.Response.Headers["Connection"] = "keep-alive";

        try {

            await foreach (var chunk in chatService.ProcessPromptAsync(id, user.userId, request.prompt)) {
                await context.Response.WriteAsync(chunk);
                await context.Response.Body.FlushAsync();
            }

            await context.Response.Body.FlushAsync();
        }
        catch (NotFoundException ex) {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsync(ex.Message);
        }
        catch (QueueException ex) {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(ex.Message);
        }
        catch (BillingException ex) {
            context.Response.StatusCode = 402;
            await context.Response.WriteAsync(ex.Message);
        }
        catch (OperationCanceledException) {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Timeout error: operation was canceled");
        }
        catch (Exception ex) {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync(ex.Message);
        }
    }

    private static async Task<IResult> GetAllUserChats(ClaimsPrincipal claimsPrincipal, IChatsRepository repository) {
        var user = claimsPrincipal.ToCurrentUser();
        
        var chats = await repository.GetAllUserChats(user.userId);
        var result = chats.Select(
            chat => new GetChatResponse(chat.Id, chat.ModelId, chat.Model.Name, chat.Model.ModelName, chat.CreatedAt, chat.UpdatedAt)
        ).ToList();
        
        return Results.Ok(result);
    }

    private static async Task<IResult> Get(ClaimsPrincipal claimsPrincipal, Guid id, IChatsRepository repository, ChatService chatService) {
        var user = claimsPrincipal.ToCurrentUser();
        var chat = await repository.Get(id);
        if (chat == null) {
            return Results.NotFound("Chat not found");
        }

        var messages = await chatService.GetChatMessagesAsync(chat.Id, user.userId);
        var responseMessages = messages.Select(message => new MessageResponse(
            message.Id,
            message.ChatId,
            message.CreatedAt,
            message.Role,
            message.Content
        ));
        
        return Results.Ok(new GetFullChatResponse(chat.Id, chat.ModelId, chat.Model.Name, chat.Model.ModelName,
            chat.CreatedAt, chat.UpdatedAt, responseMessages));
    }

    private static async Task<IResult> Delete(ClaimsPrincipal claimsPrincipal, Guid id, ChatService chatService) {
        try {
            var user = claimsPrincipal.ToCurrentUser();
            await chatService.DeleteChatAsync(id, user.userId);
            return Results.Ok(id);
        }
        catch (NullReferenceException) {
            return Results.NotFound("Chat not found");
        }
        catch (Exception) {
            return Results.InternalServerError();
        }
    }
}