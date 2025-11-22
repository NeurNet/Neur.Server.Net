using Neur.Server.Net.API.Contracts.Messages;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.API.Contracts.Chats;

public record GetFullChatResponse (
    Guid id,
    Guid model_id,
    string model_name,
    string model,
    DateTime created_at,
    DateTime? updated_at,
    IEnumerable<MessageResponse> Messages
);