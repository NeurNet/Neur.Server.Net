using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Application.Exeptions;
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
    private readonly IUsersRepository _usersRepository;
    
    public ModelService(ApplicationDbContext context, IModelsRepository modelsRepository,  IUsersRepository usersRepository) {
        _context = context;
        _modelsRepository = modelsRepository;
        _usersRepository = usersRepository;
    }
    
    public async Task<ModelEntity> CreateAsync(ModelEntity model, CancellationToken token = default) {
        await _modelsRepository.AddAsync(model,  token);
        var savedModel = await _modelsRepository.GetAsync(model.Id,  token);

        if (savedModel != null) {
            return savedModel;
        }

        throw new Exception("Error getting the model after create");
    }

    public async Task<ModelEntity?> GetAsync(Guid id,  CancellationToken token = default) {
        return await _modelsRepository.GetAsync(id, token);
    }

    public async Task<IEnumerable<ModelEntity>> GetAllByUserRoleAsync(Guid userId, CancellationToken token = default) {
        var user = await _usersRepository.GetByIdAsync(userId, false, token);
        if (user == null) {
            throw new NotFoundException("User not found");
        }
        
        var models = await _context.Models
            .AsNoTracking()
            .Where(x => user.Role == UserRole.Admin || x.Status == ModelStatus.open)
            .ToListAsync(token);
        
        return models;
    }

    public async Task UpdateAsync(ModelEntity model, CancellationToken token = default) {
        var existingModel = await _modelsRepository.GetAsync(model.Id);

        if (existingModel == null) {
            throw new Exception("Model not found");
        }

        existingModel.Name = model.Name;
        existingModel.ModelName = model.ModelName;
        existingModel.Context = model.Context;
        existingModel.Type = model.Type;
        existingModel.Version = model.Version;
        existingModel.Status = model.Status;
        existingModel.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(token);
    }

    public async Task DeleteAsync(Guid id, CancellationToken token = default) {
        await _modelsRepository.DeleteAsync(id, token);
    }
}