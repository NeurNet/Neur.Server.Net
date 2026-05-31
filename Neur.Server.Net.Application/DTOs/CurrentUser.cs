using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Interfaces;

namespace Neur.Server.Net.Application.Data;

public record CurrentUser(
    Guid userId,
    string username,
    UserRole role
);