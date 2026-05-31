using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface ISettingsRepository {
    Task SetAsync(SettingsEntity entity, CancellationToken token = default);
    Task<List<SettingsEntity>> GetAllAsync(CancellationToken token = default);
}
