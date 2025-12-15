using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.Core.Repositories;

public interface IUsersRepository {
    Task Add(UserEntity user);
    Task<UserEntity> GetByLdapId(string id);
    Task<UserEntity?> GetById(Guid id, bool tracking = false);
    Task<List<UserEntity>> GetAll();
    Task Update(UserEntity user);
}