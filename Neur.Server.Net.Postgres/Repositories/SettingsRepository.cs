using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Postgres.Repositories;

public class SettingsRepository : ISettingsRepository {
    private readonly ApplicationDbContext _db;

    public SettingsRepository(ApplicationDbContext db) {
        _db = db;
    }

    public async Task SetAsync(SettingsEntity entity, CancellationToken token = default) {
        var existing = await _db.Settings.FirstOrDefaultAsync(s => s.Name == entity.Name, token);
        if (existing is null)
            await _db.Settings.AddAsync(entity, token);
        else
            existing.JsonContent = entity.JsonContent;
    }

    public async Task<List<SettingsEntity>> GetAllAsync(CancellationToken token = default) {
        return await _db.Settings.AsNoTracking().ToListAsync(token);
    }
}
