using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text.Json;
using Neur.Server.Net.Application.Data;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Data;

namespace Neur.Server.Net.API.Extensions;

public static class ClaimsExtensions {
    public static CurrentUser ToCurrentUser(this ClaimsPrincipal principal) {
        return new CurrentUser(
            Guid.Parse(principal.FindFirst("userId")!.Value),
            principal.FindFirst("username")!.Value
        );
    }
}