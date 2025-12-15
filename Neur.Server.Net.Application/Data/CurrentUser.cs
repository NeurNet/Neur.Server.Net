using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.Application.Data;

public record CurrentUser(
    Guid userId,
    string username
);