using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IMessagesRepository {
    public Task Add(MessageEntity message);
    public Task<List<MessageEntity>> GetChatMessages(Guid chatId);
    public Task Delete(MessageEntity message);
}