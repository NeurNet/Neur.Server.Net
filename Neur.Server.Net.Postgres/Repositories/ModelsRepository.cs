using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Interfaces;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Postgres.Repositories;

public class ModelsRepository : IModelsRepository {
    private readonly ApplicationDbContext _db;
    
    public ModelsRepository(ApplicationDbContext context) {
        _db = context;
    }

    public async Task Add(ModelEntity model) {
        await _db.AddAsync(model);
        await _db.SaveChangesAsync();
    }

    public async Task Update(Guid id, string name, ModelType type, string version, ModelStatus status) {
        await _db.Models
            .Where(model => model.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Name, name)
                .SetProperty(p => p.Type, type)
                .SetProperty(p => p.Version, version)
                .SetProperty(p => p.Status, status)
                .SetProperty(p => p.UpdatedAt, DateTime.UtcNow)
            );
        await _db.SaveChangesAsync();
    }

    public async Task Delete(Guid id) {
        await _db.Models
            .Where(model => model.Id == id)
            .ExecuteDeleteAsync();
        await _db.SaveChangesAsync();
    }

    public async Task<ModelEntity?> Get(Guid id) {
        return await _db.Models
            .AsNoTracking()
            .FirstOrDefaultAsync(model => model.Id == id);
    }

    public async Task<List<ModelEntity>> GetAll() {
        return await _db.Models.AsNoTracking().ToListAsync();
    }
}