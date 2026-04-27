using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.API.Extensions;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.API.Filters;

public class UserFilter : IEndpointFilter {
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next) {
        var httpContext = context.HttpContext;
        var db = httpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
        var userInfo = httpContext.User.ToCurrentUser();

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userInfo.userId);
        if (user == null) {
            return Results.Unauthorized();
        }
        
        httpContext.Items["CurrentUser"] = user;
        
        return await next(context);
    }
}