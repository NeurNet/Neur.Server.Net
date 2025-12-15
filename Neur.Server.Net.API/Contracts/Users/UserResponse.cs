using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.API.Contracts.Users;

public record UserResponse (
    Guid user_id,
    string user_name,
    string name,
    string surname,
    UserRole role,
    int tokens
);