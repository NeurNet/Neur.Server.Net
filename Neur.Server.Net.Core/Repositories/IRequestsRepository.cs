using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IRequestsRepository {
    Task<int> Create(RequestEntity entity);
    Task<List<RequestEntity>> GetAll();
}