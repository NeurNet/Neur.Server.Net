using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.API.Contracts.Users;

public record ChangeUserRoleRequest (
    Guid user_id,
    UserRole role
);