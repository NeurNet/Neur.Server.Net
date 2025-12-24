using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Postgres.Repositories;

public class ModelsRepository : IModelsRepository {
    private readonly ApplicationDbContext _db;
    
    public ModelsRepository(ApplicationDbContext context) {
        _db = context;
    }

    public async Task AddAsync(ModelEntity model, CancellationToken token = default) {
        await _db.AddAsync(model, token);
        await _db.SaveChangesAsync(token);
    }

    public async Task DeleteAsync(Guid id, CancellationToken token = default) {
        await _db.Models
            .Where(model => model.Id == id)
            .ExecuteDeleteAsync(token);
    }

    public async Task<ModelEntity?> GetAsync(Guid id, CancellationToken token = default) {
        return await _db.Models
            .AsNoTracking()
            .FirstOrDefaultAsync(model => model.Id == id,  token);
    }

    public async Task<List<ModelEntity>> GetAllAsync(CancellationToken token = default) {
        return await _db.Models.AsNoTracking().ToListAsync(token);
    }
}