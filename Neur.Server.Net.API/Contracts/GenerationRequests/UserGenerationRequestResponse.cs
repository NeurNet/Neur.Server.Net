using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.API.Contracts.GenerationRequests;

public record UserGenerationRequestResponseItem(
    Guid id,
    Guid? model_id,
    string model_name,
    string model_ollama,
    int token_cost,
    RequestStatus status,
    DateTime created_at,
    DateTime? started_at,
    DateTime? finished_at
);

public record UserGenerationRequestResponse(
    IEnumerable<UserGenerationRequestResponseItem> items,
    int total
);
