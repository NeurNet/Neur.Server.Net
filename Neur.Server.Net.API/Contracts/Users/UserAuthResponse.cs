namespace Neur.Server.Net.API.Contracts.Users;

public record UserAuthResponse(
    string id,
    string tokens
);