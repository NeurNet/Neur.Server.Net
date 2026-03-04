namespace Neur.Server.Net.Application.Services;

public interface ITokenService {
    Task GiveTokens(Guid ownerId, Guid userId, int tokenCount);
}