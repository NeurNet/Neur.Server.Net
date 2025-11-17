using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Npgsql;

namespace Neur.Server.Net.Postgres.Repositories;

public class MessagesRepository : IMessagesRepository {
    private readonly ApplicationDbContext _db;
    
    public MessagesRepository(ApplicationDbContext context) {
        _db = context;
    }
    
    public async Task Add(MessageEntity message) {
        await _db.Messages.AddAsync(message);
        await _db.SaveChangesAsync();
    }

    public async Task<List<MessageEntity>> GetChatMessages(Guid chatId) {
        return
            await _db.Messages
                .AsNoTracking()
                .Where(m => m.ChatId == chatId)
                .ToListAsync();
    }

    public Task Delete(MessageEntity message) {
        throw new NotImplementedException();
    }
}