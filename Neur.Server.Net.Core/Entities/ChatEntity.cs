namespace Neur.Server.Net.Core.Entities;

public class ChatEntity {
    private ChatEntity() {}
    public Guid Id { get; init; }
    public Guid ModelId { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public string Context {get; set;} = string.Empty;
    
    // Навигационные свойства
    public UserEntity User { get; init; }
    public ModelEntity Model { get; private set; }

    private ChatEntity(Guid id, Guid modelId, Guid userId, DateTime createdAt, DateTime? updatedAt) {
        Id = id;
        ModelId = modelId;
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public static ChatEntity Create(Guid id, Guid modelId, Guid userId, DateTime createdAt, DateTime? updatedAt) {
        return new ChatEntity(
            id,
            modelId,
            userId,
            createdAt,
            updatedAt
        );
    }
}