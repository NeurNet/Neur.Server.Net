using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Postgres.Repositories;

public class GenerationRequestsRepository : IGenerationRequestsRepository {
    private readonly ApplicationDbContext _db;
    
    public GenerationRequestsRepository(ApplicationDbContext db) {
        _db = db;
    }
    
    public async Task<Guid> AddAsync(GenerationRequestEntity entity, CancellationToken token) {
        await _db.GenerationRequests.AddAsync(entity, token);
        return entity.Id;
    }

    public async Task<List<GenerationRequestEntity>> GetAllByRoleAsync(CancellationToken token, UserRole role) {
        return await _db.GenerationRequests
            .Include(x => x.User)
            .Include(x => x.Model)
            .Include(x => x.ResponseMessage)
            .Where(x => x.User.Role <= role)
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<GenerationRequestEntity?> GetAsync(Guid id, CancellationToken token) {
        return await _db.GenerationRequests
            .Where(x => x.Id == id)
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Model)
            .Include(x => x.ResponseMessage)
            .FirstOrDefaultAsync(token);
    }
}