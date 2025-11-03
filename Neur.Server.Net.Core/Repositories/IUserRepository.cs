using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IUserRepository {
    Task Add(UserEntity user);
    Task<UserEntity?> Get(Guid userId);
}