using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IChatsRepository {
    Task<Guid> AddAsync(ChatEntity entity, CancellationToken token = default);
    Task<List<ChatEntity>> GetAllUserChatsAsync(Guid userId, CancellationToken token = default);
    Task<ChatEntity?> GetAsync(Guid id, CancellationToken token = default);
    Task<List<ChatEntity>> GetUserModelChatsAsync(Guid userId, Guid modelId,  CancellationToken token = default);
    Task DeleteAsync(Guid id, CancellationToken token = default);
}