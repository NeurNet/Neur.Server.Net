namespace Neur.Server.Net.API.Contracts.Users;

public record UserTokensResponse (
    Guid user_id,
    int tokens
);