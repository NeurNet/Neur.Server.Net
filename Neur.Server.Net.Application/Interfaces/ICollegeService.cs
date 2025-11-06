using Neur.Server.Net.Application.Services.Contracts;

namespace Neur.Server.Net.Application.Interfaces;

public interface ILdapService {
    Task<AuthUserResponse?> AuthenticateAsync(string username, string password);
}