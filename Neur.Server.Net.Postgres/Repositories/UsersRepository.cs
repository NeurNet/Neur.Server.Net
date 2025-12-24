using Microsoft.EntityFrameworkCore;
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
        await _db.SaveChangesAsync(token);
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

    public async Task<List<UserEntity>> GetAllAsync(CancellationToken token = default) {
        var  users = await _db.Users
            .AsNoTracking()
            .ToListAsync(token);
        return users;
    }
}