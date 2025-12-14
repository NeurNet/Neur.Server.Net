using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.API.Contracts.GenerationRequests;

public record GenerationRequestResponse(
    Guid id,
    Guid model_id,
    string model_name,
    int token_cost,
    RequestStatus status,
    DateTime created_at,
    DateTime? started_at,
    DateTime? finished_at
);