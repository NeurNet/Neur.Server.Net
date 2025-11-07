using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Interfaces;

namespace Neur.Server.Net.Core.Repositories;

public interface IModelsRepository {
    Task Add(ModelEntity model);
    Task Update(Guid id, string name, ModelType type, string version, ModelStatus status);
    Task Delete(Guid id);
    Task<ModelEntity?> Get(Guid id);
    Task<List<ModelEntity>> GetAll();
}