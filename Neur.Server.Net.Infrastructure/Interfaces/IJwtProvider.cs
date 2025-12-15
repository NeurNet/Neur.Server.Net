using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Infrastructure.Interfaces;

public interface IJwtProvider {
    string GenerateToken(UserEntity user);
}