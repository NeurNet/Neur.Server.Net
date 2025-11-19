namespace Neur.Server.Net.Application.Data;

public record CurrentUser(
    Guid userId,
    string username
);