using Neur.Server.Net.Application.DTOs;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Interfaces.Services;

public interface ISettingsService {
    Task InitAsync(CancellationToken token = default);
    Task SetSettingsAsync(SettingsEntity entity, CancellationToken token = default);
    Task<List<SettingsDto>> GetSettingsAsync(CancellationToken token = default);
}
