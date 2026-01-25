using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.API.Contracts.Messages;

public record MessageResponse(
    Guid id,
    Guid chat_id,
    DateTime created_at,
    MessageRole role,
    string content
);