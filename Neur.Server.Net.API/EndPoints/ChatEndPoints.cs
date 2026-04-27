using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.Chats;
using Neur.Server.Net.API.Contracts.Messages;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.Application.Clients;
using Neur.Server.Net.Application.Data;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Application.Services.Background;
using Neur.Server.Net.Application.Services.DTO.ChatService;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Exeptions;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.API.EndPoints;

public static class ChatEndPoints {
    public static IEndpointRouteBuilder MapChatsEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/chats")
            .WithTags("Chats")
            .ProducesProblem(401)
            .RequireAuthorization();

        endpoints.MapGet(String.Empty, GetAllUserChats)
            .WithSummary("Получить список всех чатов пользователя")
            .Produces<List<GetChatResponse>>(200);

        endpoints.MapGet("/{id}", Get)
            .WithSummary("Получить чат с сообщениями")
            .Produces(404)
            .Produces<ChatWithMessagesDto>(200);
        
        endpoints.MapPost("/generate", Generate)
            .WithSummary("Сгенерировать ответ от нейросети")
            .Produces(404)
            .Produces<GenerateResponse>(200, "application/x-ndjson");
        
        endpoints.MapDelete("/{id}", Delete)
            .WithSummary("Удалить чат")
            .Produces(404)
            .Produces(200);
        
        return endpoints;
    }
    
    private static readonly JsonSerializerOptions JsonOptions = new() {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumMemberConverter(JsonNamingPolicy.CamelCase) }
    };

    private static async Task Generate([FromBody] GenerateRequest request, IChatService chatService, 
        ClaimsPrincipal claimsPrincipal, HttpContext context, CancellationToken cancellationToken) {
        
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(80));
        
        var userInfo = claimsPrincipal.ToCurrentUser();
        
        context.Response.ContentType = "application/x-ndjson";
        context.Response.Headers["Cache-Control"] = "no-cache";
        context.Response.Headers["Connection"] = "keep-alive";

        await foreach (var chunk in chatService.ProcessPromptAsync(userInfo.userId, request.ConversationId, request.ModelId, request.Message, cts.Token)) {
            var json = JsonSerializer.Serialize(chunk, JsonOptions);
            await context.Response.WriteAsync(json + "\n");
            await context.Response.Body.FlushAsync();
        }
        await context.Response.CompleteAsync();
    }

    private static async Task<IResult> GetAllUserChats(ClaimsPrincipal claimsPrincipal, IChatService chatService) {
        var user = claimsPrincipal.ToCurrentUser();
        var chats = await chatService.GetAllUserChatsAsync(user.userId);
        var result = chats.Select(chat => new GetChatResponse(chat.Id, chat.ModelId, chat.Model?.Name,
            chat.Model?.ModelName, chat.CreatedAt, chat.UpdatedAt)
        ).ToList();

        return Results.Ok(result);
    }

    private static async Task<IResult> Get(ClaimsPrincipal claimsPrincipal, Guid id, IChatService chatService) {
        var userInfo = claimsPrincipal.ToCurrentUser();
        var messages = await chatService.GetChatWithMessagesAsync(userInfo.userId, id);
        return Results.Ok(messages);
    }

    private static async Task<IResult> Delete(ClaimsPrincipal claimsPrincipal, Guid id, IChatService chatService) {
        await chatService.DeleteChatAsync(id);
        return Results.Ok(id);
    }
}