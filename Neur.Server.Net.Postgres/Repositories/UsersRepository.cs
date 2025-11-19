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
    
    public async Task Add(UserEntity user) {
        await _db.AddAsync(user);
        await _db.SaveChangesAsync();
    }

    public async Task<UserEntity> GetByLdapId(string ldapId) {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == ldapId);

        if (user == null) {
            throw new NullReferenceException("User not found");
        }
        return user;
    }

    public async Task<UserEntity> GetById(Guid id) {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) {
            throw new NullReferenceException("User not found");
        }
        return user;
    }

    public async Task Update(UserEntity user) {
        await _db.Users
            .Where(u => u.Id == user.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Username, user.Username)
                .SetProperty(x => x.Name,  user.Name)
                .SetProperty(x => x.Surname, user.Surname)
                .SetProperty(x => x.Tokens, user.Tokens)
                .SetProperty(x => x.Role, user.Role)
            );
        await _db.SaveChangesAsync();
    }
}