namespace Neur.Server.Net.Application.Services.Contracts;

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