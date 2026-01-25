namespace Neur.Server.Net.Infrastructure.Clients.Contracts.CollegeClient;

public record AuthUserResponse(
    string id,
    string role,
    string username
);

public record AuthResponse(
    string access_token,
    string refresh_token,
    AuthUserResponse user
);