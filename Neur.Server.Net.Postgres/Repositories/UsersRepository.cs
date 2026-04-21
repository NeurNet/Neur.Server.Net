using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Postgres.Repositories;

public class UsersRepository : IUsersRepository {
    private readonly ApplicationDbContext _db;
    
    public UsersRepository(ApplicationDbContext context) {
        _db = context;
    }
    
    public async Task AddAsync(UserEntity user, CancellationToken token = default) {
        await _db.AddAsync(user,  token);
    }

    public async Task<UserEntity> GetByLdapIdAsync(string ldapId, CancellationToken token = default) {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == ldapId,  token);

        if (user == null) {
            throw new NullReferenceException("User not found");
        }
        return user;
    }

    public async Task<UserEntity?> GetByIdAsync(Guid id, bool tracking = false, CancellationToken token = default) {
        var query = tracking ? _db.Users 
            : _db.Users.AsNoTracking();
        
        var user = await query
            .FirstOrDefaultAsync(u => u.Id == id,  token);
        
        return user;
    }

    public async Task<List<(UserEntity User, DateTime? LastRequestTime)>> GetAllWithLastRequestTimeAsync(CancellationToken token = default) {
        var raw = await _db.Users
            .GroupJoin(
                _db.GenerationRequests,
                u => u.Id,
                r => r.UserId,
                (u, r) => new {
                    User = u,
                    LastRequestTime = r.OrderByDescending(req => req.CreatedAt)
                        .Select(req => (DateTime?)req.CreatedAt)
                        .FirstOrDefault()
                }
            )
            .ToListAsync(token);

        return raw.Select(u => (u.User, u.LastRequestTime)).ToList();
    }

    public async Task UpdateRoleAsync(Guid id, UserRole role, CancellationToken token = default) {
        await _db.Users
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Role, role), token);
    }
}