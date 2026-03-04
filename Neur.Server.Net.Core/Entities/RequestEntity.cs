using Neur.Server.Net.Core.Interfaces;

namespace Neur.Server.Net.Core.Entities;

public class RequestEntity {
    public int Id { get; init; }
    public Guid ChatId { get; init; }
    public string Promt {get; init; }
    public string? Response {get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? FinishedAt { get; init; }
    
    private RequestEntity() {}

    private RequestEntity(Guid chatId, string promt, string response, DateTime createdAt, DateTime finishedAt) {
        ChatId = chatId;
        Promt = promt;
        Response = response;
        CreatedAt = createdAt;
        FinishedAt = finishedAt;
    }

    public static RequestEntity Create(Guid chatId, string promt, string response, DateTime createdAt, DateTime finishedAt) {
        return new RequestEntity(
            chatId,
            promt,
            response,
            createdAt,
            finishedAt
        );
    }
}