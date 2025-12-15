using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IChatsRepository {
    Task<Guid> Add(ChatEntity entity);
    Task<List<ChatEntity>> GetAllUserChats(Guid userId);
    Task<ChatEntity?> Get(Guid id);
    Task<List<ChatEntity>> GetUserModelChats(Guid userId, Guid modelId);
    Task Delete(Guid id);
}