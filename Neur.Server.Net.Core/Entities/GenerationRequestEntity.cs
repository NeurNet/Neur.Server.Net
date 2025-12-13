using Neur.Server.Net.Core.Abstractions;
using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Core.Entities;

public class GenerationRequestEntity : Entity {
    public Guid UserId { get; init; }
    public Guid ModelId { get; init; }
    public int TokenCost { get; init; } 
    public DateTime CreatedAt { get; private init; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public RequestStatus Status { get; private set; }
    
    public UserEntity User { get; private set; }
    public ModelEntity Model { get; private set; }
    
    private GenerationRequestEntity() {}

    public GenerationRequestEntity(Guid userId, Guid modelId, DateTime createdAt) {
        Id = Guid.NewGuid();
        UserId = userId;
        ModelId = modelId;
        Status = RequestStatus.Pending;
        CreatedAt = createdAt;
    }

    public void Start() {
        StartedAt = DateTime.UtcNow;
        Status = RequestStatus.InProgress;
    }

    public void Finish() => FinishedAt = DateTime.UtcNow;
    public void MarkFailed() =>  Status = RequestStatus.Failed;
}