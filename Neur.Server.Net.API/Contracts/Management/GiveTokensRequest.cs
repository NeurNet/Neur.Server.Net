namespace Neur.Server.Net.API.Contracts.Management;

public record GiveTokensRequest(
    Guid user_id,
    int token_count
);