using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Application.Exceptions;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services;

public class GenerationRequestService {
    private readonly IGenerationRequestsRepository _requestsRepository;
    private readonly IUsersRepository _usersRepository;
    
    public GenerationRequestService(IGenerationRequestsRepository requestsRepository, IUsersRepository usersRepository) {
        _requestsRepository = requestsRepository;
        _usersRepository = usersRepository;
    }

    public async Task<(IEnumerable<GenerationRequestEntity>, int)> GetPageAsync(Guid userId, int page, int pageSize,  CancellationToken cancellationToken) {
        var user = await _usersRepository.GetByIdAsync(userId, token: cancellationToken);
        if (user == null) {
            throw new NotFoundException("User not found");
        }
        var requests = await _requestsRepository.GetPageByRoleAsync(page, pageSize, user.Role, cancellationToken);
        var totalCount = await _requestsRepository.GetCountByRoleAsync(user.Role, cancellationToken);
        return (requests, totalCount);
    }

    public async Task<(IEnumerable<GenerationRequestEntity>, int)> GetUserPageAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken) {
        var user = await _usersRepository.GetByIdAsync(userId, token: cancellationToken);
        if (user == null) {
            throw new NotFoundException("User not found");
        }
        var requests = await _requestsRepository.GetUserPageAsync(userId, page, pageSize, cancellationToken);
        var totalCount = await _requestsRepository.GetUserCountAsync(userId, cancellationToken);
        return (requests, totalCount);
    }
}