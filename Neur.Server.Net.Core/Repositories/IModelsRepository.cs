using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.Core.Repositories;

public interface IModelsRepository {
    Task AddAsync(ModelEntity model,  CancellationToken token = default);
    Task DeleteAsync(ModelEntity model, CancellationToken token = default);
    Task<ModelEntity?> GetAsync(Guid id, CancellationToken token = default);
    Task<List<ModelEntity>> GetAllAsync(CancellationToken token = default);
    Task<int> GetCountAsync(CancellationToken token = default);
}