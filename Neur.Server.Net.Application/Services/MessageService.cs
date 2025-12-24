using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services;

public class MessageService : IMessageService {
    private IMessagesRepository  _messagesRepository;
    
    public MessageService(IMessagesRepository messagesRepository) {
        _messagesRepository  = messagesRepository;;
    }
    
    public async Task<Guid> SaveMessageAsync(ChatEntity chat, MessageRole role, string content) {
        var messageEntity = new MessageEntity(chat.Id, DateTime.UtcNow, role, content);
        await _messagesRepository.AddAsync(messageEntity);
        return messageEntity.Id;
    }
}