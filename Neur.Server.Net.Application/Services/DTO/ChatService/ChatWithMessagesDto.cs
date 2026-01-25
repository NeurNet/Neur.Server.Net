namespace Neur.Server.Net.Application.Services.DTO.ChatService;

public record ChatWithMessagesDto (
    Guid id, 
    Guid model_id,
    string model_name,
    string model,
    DateTime created_at,
    DateTime? updated_at,
    IEnumerable<MessageDto> Messages
);