using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.API.Extensions;

public static class HttpContextExtensions {
    public static UserEntity GetUser(this HttpContext ctx) {
        return (UserEntity) ctx.Items["CurrentUser"]!;
    }
}