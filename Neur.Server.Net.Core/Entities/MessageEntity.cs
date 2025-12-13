using Neur.Server.Net.Core.Abstractions;
using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Core.Entities;

public class MessageEntity : Entity {
    public Guid ChatId { get; init; }
    public DateTime CreatedAt { get; init; }
    public MessageRole Role { get; init; }
    public string Content { get; set; } = string.Empty;
    public ChatEntity Chat { get; private set; }

    private MessageEntity() {}

    public MessageEntity(Guid chatId, DateTime createdAt, MessageRole role, string content) {
        Id = Guid.NewGuid();
        ChatId = chatId;
        CreatedAt = createdAt;
        Role = role;
        Content = content;
    }
}