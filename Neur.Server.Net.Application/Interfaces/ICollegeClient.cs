using Neur.Server.Net.Application.Services.Contracts;
using Neur.Server.Net.Application.Services.Contracts.CollegeService;

namespace Neur.Server.Net.Application.Interfaces;

public interface ICollegeClient {
    Task<AuthUserResponse?> AuthenticateAsync(string username, string password);
}