namespace Neur.Server.Net.Application.Interfaces.Services;

public interface IDashboardService {
    Task<(int requestsCount, int usersCount, int? modelsCount)> GetAsync(Guid userId, CancellationToken token = default);
}
