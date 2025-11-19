using System.Security.Claims;
using Neur.Server.Net.Application.Data;

namespace Neur.Server.Net.API.Extensions;

public static class ClaimsExtensions {
    public static CurrentUser ToCurrentUser(this ClaimsPrincipal principal) {
        return new CurrentUser(
            Guid.Parse(principal.FindFirst("userId")!.Value),
            principal.FindFirst("username")!.Value
        );
    }
}