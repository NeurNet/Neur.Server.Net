using Neur.Server.Net.Core.Abstractions;

namespace Neur.Server.Net.Core.Entities;

public class ChatEntity : Entity {
    public Guid ModelId { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Навигационные свойства
    public UserEntity User { get; init; }
    public ModelEntity Model { get; private set; }
    
    public ChatEntity(Guid modelId, Guid userId, DateTime createdAt, DateTime? updatedAt = null) {
        Id = Guid.NewGuid();
        ModelId = modelId;
        UserId = userId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
    
    private ChatEntity() {}
}