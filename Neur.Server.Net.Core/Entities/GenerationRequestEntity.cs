using Neur.Server.Net.Core.Abstractions;
using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Core.Entities;

public class GenerationRequestEntity : Entity {
    public Guid UserId { get; init; }
    public Guid ModelId { get; init; }
    public Guid PromptMessageId { get; init; }
    public Guid? ResponseMessageId { get; private set; }
    public int TokenCost { get; private init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }
    public RequestStatus Status { get; private set; }
    
    public UserEntity User { get; private set; }
    public ModelEntity Model { get; private set; }
    public MessageEntity PromptMessage { get; private set; }
    public MessageEntity? ResponseMessage { get; private set; }
    
    private GenerationRequestEntity() {}

    public GenerationRequestEntity(Guid userId, Guid modelId, int tokenCost, Guid promptMessageId) {
        Id = Guid.NewGuid();
        UserId = userId;
        ModelId = modelId;
        TokenCost = tokenCost;
        PromptMessageId = promptMessageId;
        Status = RequestStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkInProcess() {
        StartedAt = DateTime.UtcNow;
        Status = RequestStatus.InProgress;
    }

    public void MarkSuccessFinished(Guid responseMessageId) {
        FinishedAt = DateTime.UtcNow;
        ResponseMessageId = responseMessageId;
        Status = RequestStatus.Success;
    }

    public void MarkFailed() {
        FinishedAt =  DateTime.UtcNow;
        Status = RequestStatus.Failed;
    }
}