using System.Text.Json;
using Microsoft.Extensions.Options;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Application.Options;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Application.Services;

public class SettingsService : ISettingsService {
    private readonly ISettingsRepository _settingsRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly SettingsOptions _options;
    private readonly IOllamaClient _ollamaClient;
    private readonly ICollegeClient _collegeClient;

    public SettingsService(
        ISettingsRepository settingsRepository,
        IUnitOfWork unitOfWork,
        IOptions<SettingsOptions> options,
        IOllamaClient ollamaClient,
        ICollegeClient collegeClient) {
        _settingsRepository = settingsRepository;
        _unitOfWork = unitOfWork;
        _options = options.Value;
        _ollamaClient = ollamaClient;
        _collegeClient = collegeClient;
    }

    public async Task InitAsync(CancellationToken token = default) {
        var existing = await _settingsRepository.GetAllAsync(token);
        if (existing.Count == 0) {
            var ollamaEntity = new SettingsEntity(SettingName.Ollama, JsonSerializer.Serialize(_options.OllamaClient));
            var authEntity = new SettingsEntity(SettingName.Auth, JsonSerializer.Serialize(_options.CollegeClient));
            await _settingsRepository.SetAsync(ollamaEntity, token);
            await _settingsRepository.SetAsync(authEntity, token);
            await _unitOfWork.SaveChangesAsync(token);
            existing = [ollamaEntity, authEntity];
        }

        foreach (var setting in existing)
            ApplyToClient(setting);
    }

    public async Task SetSettingsAsync(SettingsEntity entity, CancellationToken token = default) {
        await _settingsRepository.SetAsync(entity, token);
        await _unitOfWork.SaveChangesAsync(token);
        ApplyToClient(entity);
    }

    public async Task<List<SettingsEntity>> GetSettingsAsync(CancellationToken token = default) {
        return await _settingsRepository.GetAllAsync(token);
    }

    private void ApplyToClient(SettingsEntity entity) {
        switch (entity.Name) {
            case SettingName.Ollama:
                _ollamaClient.SetOptions(JsonSerializer.Deserialize<OllamaSettingsOptions>(entity.Content)!);
                break;
            case SettingName.Auth:
                _collegeClient.SetOptions(JsonSerializer.Deserialize<AuthSettingsOptions>(entity.Content)!);
                break;
        }
    }
}
