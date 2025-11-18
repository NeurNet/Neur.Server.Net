using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.API.Contracts.Messages;

public record GetMessageResponse(
    DateTime created_at,
    MessageRole role,
    string content
);