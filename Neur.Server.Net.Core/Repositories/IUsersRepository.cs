using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Core.Repositories;

public interface IUsersRepository {
    Task Add(UserEntity user);
    Task<UserEntity?> GetByLdapId(string id);
}