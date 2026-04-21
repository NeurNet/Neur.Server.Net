using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.API.Contracts.Users;

public record UserWithLastRequestResponse(
    Guid user_id,
    string user_name,
    string name,
    string surname, 
    UserRole role,
    int tokens,
    DateTime? last_request
);