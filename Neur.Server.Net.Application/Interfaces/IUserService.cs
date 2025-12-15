using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Interfaces;

public interface IUserService {
    Task<string> Login(string username, string password);
    Task<List<UserEntity>> GetAllUsers();
}