using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Application.Services.DTO.ChatService;

public record MessageDto (
    Guid id,
    Guid chat_id,
    DateTime created_at,
    MessageRole role,
    string content
);