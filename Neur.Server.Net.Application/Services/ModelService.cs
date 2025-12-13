using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services;

public class ModelService : IModelService {
    private readonly ApplicationDbContext _context;
    private readonly IModelsRepository _modelsRepository;
    
    public ModelService(ApplicationDbContext context, IModelsRepository modelsRepository) {
        _context = context;
        _modelsRepository = modelsRepository;
    }
    
    public ModelEntity CreateAsync(ModelEntity model) {
        throw new NotImplementedException();
    }

    public ModelEntity GetAsync(Guid id) {
        throw new NotImplementedException();
    }

    public IEnumerable<ModelEntity> GetAllByRoleAsync(UserRole role) {
        throw new NotImplementedException();
    }

    public void UpdateAsync(ModelEntity model) {
        throw new NotImplementedException();
    }

    public void DeleteAsync(Guid id) {
        throw new NotImplementedException();
    }
}