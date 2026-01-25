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
    
    public async Task AddAsync(MessageEntity message, CancellationToken token = default) {
        await _db.Messages.AddAsync(message, token);
        await _db.SaveChangesAsync(token);
    }

    public async Task<List<MessageEntity>> GetChatMessagesAsync(Guid chatId, CancellationToken token = default) {
        return
            await _db.Messages
                .AsNoTracking()
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(token);
    }

    public Task DeleteAsync(MessageEntity message, CancellationToken token = default) {
        throw new NotImplementedException();
    }
}