using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IMessagesRepository {
    public Task AddAsync(MessageEntity message, CancellationToken token = default);
    public Task<List<MessageEntity>> GetChatMessagesAsync(Guid chatId, CancellationToken token = default);
    public Task DeleteAsync(MessageEntity message, CancellationToken token = default);
}