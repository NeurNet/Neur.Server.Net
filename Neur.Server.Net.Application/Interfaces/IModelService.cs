using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.Application.Interfaces;

public interface IModelService {
    public Task<ModelEntity> CreateAsync(ModelEntity model);
    public Task<ModelEntity?> GetAsync(Guid id);
    public Task<IEnumerable<ModelEntity>> GetAllByUserRoleAsync(Guid userId);
    public Task UpdateAsync(ModelEntity model);
    public Task DeleteAsync(Guid id);
}