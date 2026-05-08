using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Interfaces;

public interface IUserService {
    Task<string> Login(string username, string password, CancellationToken cancellationToken = default);
    Task<List<(UserEntity, DateTime?)>> GetAllUsersWithLastRequest(CancellationToken token = default);
    Task ChangeUserRole(Guid actorId, Guid userId, UserRole role);
}