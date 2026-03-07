using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Interfaces.Services;

public interface IModelService {
    public Task<ModelEntity> CreateAsync(ModelEntity model, CancellationToken token = default);
    public Task<ModelEntity> GetAsync(Guid id, CancellationToken token = default);
    public Task<IEnumerable<ModelEntity>> GetAllByUserRoleAsync(Guid userId, CancellationToken token = default);
    public Task UpdateAsync(ModelEntity model, CancellationToken token = default);
    public Task DeleteAsync(Guid id, CancellationToken token = default);
}