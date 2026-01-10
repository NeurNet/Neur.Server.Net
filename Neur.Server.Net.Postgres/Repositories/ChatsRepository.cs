using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Exeptions;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Postgres.Repositories;

public class ChatsRepository : IChatsRepository {
    private readonly ApplicationDbContext _db;
    
    public ChatsRepository(ApplicationDbContext db) {
        _db = db;
    }
    
    public async Task<Guid> AddAsync(ChatEntity entity, CancellationToken token = default) {
        try {
            await _db.Chats.AddAsync(entity);
            await _db.SaveChangesAsync(token);
            return entity.Id;
        }
        catch (DbUpdateException ex) {
            throw new CreatingEntityException();
        }
    }

    public async Task<List<ChatEntity>> GetAllUserChatsAsync(Guid userId, CancellationToken token = default) {
        return 
            await _db.Chats
                .AsNoTracking()
                .Include(x => x.Model)
                .ToListAsync(token);
    }

    public async Task<ChatEntity?> GetAsync(Guid id, CancellationToken token = default) {
        return 
            await _db.Chats
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Include(x => x.User)
                .Include(x => x.Model)
                .FirstOrDefaultAsync(token);
    }

    public async Task<List<ChatEntity>> GetUserModelChatsAsync(Guid userId, Guid modelId, CancellationToken token = default) {
        return 
            await _db.Chats
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.ModelId == modelId)
                .Include(x => x.Model)
                .ToListAsync(token);
    }
    public async Task DeleteAsync(Guid id,  CancellationToken token = default) {
        var chat = await _db.Chats.Where(x => x.Id == id).FirstOrDefaultAsync(token);
        if (chat != null) {
            _db.Chats.Remove(chat);
            await _db.SaveChangesAsync(token);
        }
        else {
            throw new NullReferenceException();
        }
    }
}