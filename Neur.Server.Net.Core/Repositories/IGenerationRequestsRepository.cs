using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IGenerationRequestsRepository {
    Task<Guid> AddAsync(GenerationRequestEntity entity, CancellationToken token);
    Task<List<GenerationRequestEntity>> GetAllByRoleAsync(CancellationToken token, UserRole role);

    Task<List<GenerationRequestEntity>> GetPartByRoleAsync(int page, int pageSize, UserRole role, CancellationToken token);
    Task<int> GetCountByRoleAsync(UserRole role, CancellationToken token);
    Task<GenerationRequestEntity?> GetAsync(Guid id, CancellationToken token);
}