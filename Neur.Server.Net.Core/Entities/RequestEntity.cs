using Neur.Server.Net.Core.Interfaces;

namespace Neur.Server.Net.Core.Entities;

public class RequestEntity {
    public Guid Id { get; init; }
    public Guid ChatId { get; init; }
    public string Prompt {get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    
    public ChatEntity Chat {get; init; }
    
    private RequestEntity() {}

    private RequestEntity(Guid id, Guid chatId, string prompt, string? response, DateTime createdAt, DateTime? startedAt, DateTime? finishedAt) {
        Id = id;
        ChatId = chatId;
        Prompt = prompt;
        CreatedAt = createdAt;
        StartedAt = startedAt;
        FinishedAt = finishedAt;
    }

    public static RequestEntity Create(Guid id, Guid chatId, string promt, DateTime createdAt) {
        return new RequestEntity(
            id,
            chatId,
            promt,
            null,
            createdAt,
            null,
            null
        );
    }
}