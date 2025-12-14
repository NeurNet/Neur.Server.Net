using Neur.Server.Net.Application.Services.DTO.ChatService;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Extensions;

public static class ChatMapper {
    public static ChatWithMessagesDto ToResponse(this ChatEntity chatEntity, IEnumerable<MessageEntity> messageEntities) {
        return new ChatWithMessagesDto(
            chatEntity.Id,
            chatEntity.ModelId,
            chatEntity.Model.ModelName,
            chatEntity.Model.Name,
            chatEntity.CreatedAt,
            chatEntity.UpdatedAt,
            messageEntities.ToResponse()
        );
    }
    
    public static IEnumerable<ChatWithMessagesDto> ToResponse(this IEnumerable<ChatEntity> chatEntities, IEnumerable<MessageEntity> messageEntities) {
        return chatEntities.Select(x => x.ToResponse(messageEntities));
    }
}