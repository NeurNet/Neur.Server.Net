using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Interfaces;

public interface IUserService {
    Task<string> Login(string username, string password);
    Task<List<UserEntity>> GetAllUsers();
    Task ChangeUserRole(Guid userId, UserRole role);
}