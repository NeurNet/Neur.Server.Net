using Neur.Server.Net.Application.Options;
using Neur.Server.Net.Infrastructure.Clients.Contracts.CollegeClient;

namespace Neur.Server.Net.Application.Interfaces.Clients;

public interface ICollegeClient {
    void SetOptions(AuthSettingsContent options);
    Task<AuthUserResponse?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
}