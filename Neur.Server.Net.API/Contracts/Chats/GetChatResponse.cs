namespace Neur.Server.Net.API.Contracts.Chats;

public record GetChatResponse(
    Guid id,
    Guid model_id,
    string model_name,
    string model,
    DateTime created_at,
    DateTime? updated_at
);