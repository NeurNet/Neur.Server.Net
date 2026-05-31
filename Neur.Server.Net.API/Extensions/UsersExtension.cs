using Neur.Server.Net.API.Contracts.Users;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.API.Extensions;

public static class UsersExtension {
    public static IEnumerable<UserWithLastRequestResponse> ToResponse(this List<(UserEntity, DateTime?)> source) {
        return source.Select(x => new UserWithLastRequestResponse(
            x.Item1.Id,
            x.Item1.Username,
            x.Item1.Name,
            x.Item1.Surname,
            x.Item1.Role,
            x.Item1.Tokens,
            x.Item2
        ));
    }
}