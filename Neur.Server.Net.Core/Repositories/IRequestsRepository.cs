using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IRequestsRepository {
    Task<Guid> Add(RequestEntity entity);
    
    Task Update(RequestEntity entity);
    Task<List<RequestEntity>> GetAll();
    Task<RequestEntity?> Get(Guid id);
}