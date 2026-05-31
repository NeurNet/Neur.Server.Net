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

    public async Task<List<GenerationRequestEntity>> GetPageByRoleAsync(int page, int pageSize, UserRole role, CancellationToken token) {
        return await _db.GenerationRequests
            .OrderByDescending(x => x.CreatedAt)
            .Include(x => x.User)
            .Include(x => x.ResponseMessage)
            .Where(x => x.User.Role <= role)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<int> GetCountByRoleAsync(UserRole role, CancellationToken token) {
        return await _db.GenerationRequests
            .Include(x => x.User)
            .Where(x => x.User.Role <= role)
            .CountAsync(token);
    }

    public async Task<List<GenerationRequestEntity>> GetUserPageAsync(Guid userId, int page, int pageSize, CancellationToken token) {
        return await _db.GenerationRequests
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Include(x => x.User)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<int> GetUserCountAsync(Guid userId, CancellationToken token) {
        return await _db.GenerationRequests
            .CountAsync(x => x.UserId == userId, token);
    }
}