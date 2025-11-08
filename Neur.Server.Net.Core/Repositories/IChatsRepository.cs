using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IChatsRepository {
    Task<Guid> Create(ChatEntity entity);
    Task<List<ChatEntity>> GetAllUserChats(Guid userId);
    Task<List<ChatEntity>> GetUserModelChats(Guid userId, Guid modelId);
    Task Delete(Guid id);
}