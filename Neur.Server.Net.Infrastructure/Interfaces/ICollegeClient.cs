using Neur.Server.Net.Infrastructure.Clients.Contracts.CollegeClient;

namespace Neur.Server.Net.Infrastructure.Interfaces;

public interface ICollegeClient {
    Task<AuthUserResponse?> AuthenticateAsync(string username, string password);
}