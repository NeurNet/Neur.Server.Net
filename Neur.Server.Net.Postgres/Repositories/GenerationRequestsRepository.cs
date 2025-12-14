using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Postgres.Repositories;

public class GenerationRequestsRepository : IGenerationRequestsRepository {
    private readonly ApplicationDbContext _db;
    
    public GenerationRequestsRepository(ApplicationDbContext db) {
        _db = db;
    }
    
    public async Task<Guid> Add(GenerationRequestEntity entity) {
        await _db.GenerationRequests.AddAsync(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task Update(GenerationRequestEntity entity) {
        await _db.GenerationRequests
            .Where(req => req.Id == entity.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.StartedAt, entity.StartedAt)
                .SetProperty(p => p.FinishedAt, entity.FinishedAt)
            );
        await _db.SaveChangesAsync();
    }

    public async Task<List<GenerationRequestEntity>> GetAll() {
        return await _db.GenerationRequests.AsNoTracking().ToListAsync();
    }

    public async Task<GenerationRequestEntity?> Get(Guid id) {
        return await _db.GenerationRequests
            .Where(x => x.Id == id)
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Model)
            .FirstOrDefaultAsync();
    }
}