using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Postgres.Repositories;

public class UserRepository : IUserRepository {
    private readonly ApplicationDbContext _db;
    
    public UserRepository(ApplicationDbContext context) {
        _db = context;
    }
    
    public async Task Add(UserEntity user) {
        await _db.AddAsync(user);
        await _db.SaveChangesAsync();
    }

    public async Task<UserEntity?> GetByLdapId(string ldapId) {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == ldapId);
    }
}