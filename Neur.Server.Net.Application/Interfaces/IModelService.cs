using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.Application.Interfaces;

public interface IModelService {
    public ModelEntity CreateAsync(ModelEntity model);
    public ModelEntity GetAsync(Guid id);
    public IEnumerable<ModelEntity> GetAllByRoleAsync(UserRole role);
    public void UpdateAsync(ModelEntity model);
    public void DeleteAsync(Guid id);
}