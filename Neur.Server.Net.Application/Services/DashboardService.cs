using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Application.Services;

public class DashboardService : IDashboardService {
    private readonly IUsersRepository _usersRepository;
    private readonly IGenerationRequestsRepository _requestsRepository;
    private readonly IModelsRepository _modelsRepository;

    public DashboardService(
        IUsersRepository usersRepository,
        IGenerationRequestsRepository requestsRepository,
        IModelsRepository modelsRepository) {
        _usersRepository = usersRepository;
        _requestsRepository = requestsRepository;
        _modelsRepository = modelsRepository;
    }

    public async Task<(int requestsCount, int usersCount, int? modelsCount)> GetAsync(Guid userId, CancellationToken token = default) {
        var user = await _usersRepository.GetByIdAsync(userId, token: token);
        if (user == null) {
            throw new NotFoundException("User not found");
        }

        var requestsCount = await _requestsRepository.GetCountByRoleAsync(user.Role, token);
        var usersCount = await _usersRepository.GetCountAsync(token);
        int? modelsCount = user.Role == UserRole.Admin
            ? await _modelsRepository.GetCountAsync(token)
            : null;

        return (requestsCount, usersCount, modelsCount);
    }
}
