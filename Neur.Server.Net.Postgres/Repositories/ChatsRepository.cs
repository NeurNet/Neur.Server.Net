using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Postgres.Repositories;

public class ChatsRepository : IChatsRepository {
    private readonly ApplicationDbContext _db;
    
    public ChatsRepository(ApplicationDbContext db) {
        _db = db;
    }
    
    public async Task<Guid> Add(ChatEntity entity) {
        await _db.Chats.AddAsync(entity);
        await _db.SaveChangesAsync();
        return entity.Id;
    }

    public async Task<List<ChatEntity>> GetAllUserChats(Guid userId) {
        return 
            await _db.Chats
                .AsNoTracking()
                .Include(x => x.Model)
                .ToListAsync();
    }

    public async Task<ChatEntity?> Get(Guid id) {
        return 
            await _db.Chats
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Include(x => x.Model)
                .FirstOrDefaultAsync();
    }

    public async Task<List<ChatEntity>> GetUserModelChats(Guid userId, Guid modelId) {
        return 
            await _db.Chats
                .AsNoTracking()
                .Where(x => x.UserId == userId && x.ModelId == modelId)
                .Include(x => x.Model)
                .ToListAsync();
    }
    public async Task Delete(Guid id) {
        var chat = await _db.Chats.Where(x => x.Id == id).FirstOrDefaultAsync();
        if (chat != null) {
            _db.Chats.Remove(chat);
            await _db.SaveChangesAsync();
        }
        else {
            throw new NullReferenceException();
        }
    }
}