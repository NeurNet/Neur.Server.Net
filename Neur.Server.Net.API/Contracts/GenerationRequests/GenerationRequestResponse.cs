using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.API.Contracts.GenerationRequests;


public record GenerationRequestMessageResponse(
    Guid id,
    MessageRole role,
    string content
);
public record GenerationRequestUserResponse(
    Guid id,
    string username,
    string name,
    string surname
);

public record GenerationRequestResponseItem(
    Guid id,
    Guid model_id,
    string model_name,
    int token_cost,
    RequestStatus status,
    DateTime created_at,
    DateTime? started_at,
    DateTime? finished_at,
    GenerationRequestUserResponse user,
    GenerationRequestMessageResponse? message
);

public record GenerationRequestResponse(
    IEnumerable<GenerationRequestResponseItem> items,
    int total
);