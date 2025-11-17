using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Core.Entities;

public class MessageEntity {
    public Guid Id { get; init; }
    public Guid ChatId { get; init; }
    public DateTime CreatedAt { get; init; }
    public MessageRole Role { get; init; }
    public string Content { get; set; } = string.Empty;
    public ChatEntity Chat { get; private set; }

    private MessageEntity() {}

    private MessageEntity(Guid id, Guid chatId, DateTime createdAt, MessageRole role, string content) {
        Id = id;
        ChatId = chatId;
        CreatedAt = createdAt;
        Role = role;
        Content = content;
    }

    public MessageEntity Create(Guid chatId, DateTime createdAt, MessageRole role, string content) {
        return new MessageEntity(Guid.NewGuid(), chatId, createdAt, role, content);
    }
}