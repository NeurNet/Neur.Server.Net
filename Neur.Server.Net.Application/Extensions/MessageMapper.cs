using Neur.Server.Net.Application.Services.DTO.ChatService;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Extensions;

public static class MessageMapper {
    public static MessageDto ToResponse(this MessageEntity message) {
        return new MessageDto(
            message.Id,
            message.ChatId,
            message.CreatedAt,
            message.Role,
            message.Content
        );
    }

    public static IEnumerable<MessageDto> ToResponse(this IEnumerable<MessageEntity> messages) {
        return messages.Select(ToResponse);
    }
}