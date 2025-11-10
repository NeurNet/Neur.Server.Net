using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IRequestsRepository {
    Task<int> Add(RequestEntity entity);
    
    Task Update(int id, string response, DateTime finishedAt);
    Task<List<RequestEntity>> GetAll();
}