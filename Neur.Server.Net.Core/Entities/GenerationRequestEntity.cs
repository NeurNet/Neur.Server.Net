using Neur.Server.Net.Core.Abstractions;
using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Core.Entities;

public class GenerationRequestEntity : Entity {
    public Guid UserId { get; init; }
    public Guid ModelId { get; init; }
    public int TokenCost { get; init; }
    public string Prompt { get; init; }
    public DateTime CreatedAt { get; private init; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public RequestStatus Status { get; set; }
    
    public UserEntity User { get; private set; }
    public ModelEntity Model { get; private set; }
    
    private GenerationRequestEntity() {}

    public GenerationRequestEntity(Guid userId, Guid modelId, int tokencost, string prompt, DateTime createdAt) {
        Id = Guid.NewGuid();
        UserId = userId;
        ModelId = modelId;
        TokenCost = tokencost;
        Prompt = prompt;
        Status = RequestStatus.Pending;
        CreatedAt = createdAt;
    }
}