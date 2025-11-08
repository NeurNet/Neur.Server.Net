namespace Neur.Server.Net.Core.Entities;

public class ChatEntity {
    private ChatEntity() {}
    
    public Guid Id { get; init; }
    public Guid ModelId { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    
    public UserEntity User { get; init; }
    public ModelEntity Model { get; init; }
}