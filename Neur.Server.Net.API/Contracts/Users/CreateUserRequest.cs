namespace Neur.Server.Net.API.Contracts.Users;

public record CreateUserRequest (
    string username,
    string password
);