using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Postgres.Repositories;

public class RequestsRepository : IRequestsRepository {
    private readonly ApplicationDbContext _db;
    
    public RequestsRepository(ApplicationDbContext db) {
        _db = db;
    }
    
    public async Task<int> Add(RequestEntity entity) {
        await _db.Requests.AddAsync(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task Update(int id, string response, DateTime finishedAt) {
        await _db.Requests
            .Where(req => req.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Response, response)
                .SetProperty(p => p.FinishedAt, finishedAt)
            );
        await _db.SaveChangesAsync();
    }

    public async Task<List<RequestEntity>> GetAll() {
        return await _db.Requests.AsNoTracking().ToListAsync();
    }
}