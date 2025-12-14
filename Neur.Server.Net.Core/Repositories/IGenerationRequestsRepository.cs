using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IGenerationRequestsRepository {
    Task<Guid> Add(GenerationRequestEntity entity);
    
    Task Update(GenerationRequestEntity entity);
    Task<List<GenerationRequestEntity>> GetAll();
    Task<GenerationRequestEntity?> Get(Guid id);
}