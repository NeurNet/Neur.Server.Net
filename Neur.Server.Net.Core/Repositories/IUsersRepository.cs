using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.Core.Repositories;

public interface IUsersRepository {
    Task AddAsync(UserEntity user, CancellationToken token = default);
    Task<UserEntity> GetByLdapIdAsync(string id, CancellationToken token = default);
    Task<UserEntity?> GetByIdAsync(Guid id, bool tracking = false, CancellationToken token = default);
    Task<List<UserEntity>> GetAllAsync(CancellationToken token = default);
}