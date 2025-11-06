using System.Security.Claims;
using Neur.Server.Net.API.Contracts.Users;
using Neur.Server.Net.Application.Services;

namespace Neur.Server.Net.API.EndPoints;

public static class UserEndPoints {
    public static IEndpointRouteBuilder MapUserEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/users/auth");
        
        endpoints.MapPost(String.Empty, Login);
        endpoints.MapGet(String.Empty, Auth).RequireAuthorization();
        
        return endpoints;
    }
    
    private static async Task<IResult> Login(UserLoginRequest req, UserService userService, HttpResponse response) {
        try {
            var token = await userService.Login(req.username, req.password);
            response.Cookies.Append("auth_token", token, new CookieOptions {
                HttpOnly = true,
                Secure = true, // если HTTPS
                SameSite = SameSiteMode.Strict
            });

            return Results.Ok(token);
        }
        catch (NullReferenceException) {
            return Results.NotFound();
        }
        catch (Exception ex) {
            return Results.BadRequest(ex.Message);
        }
    }
    
    public static async Task<IResult> Auth(ClaimsPrincipal user) {
        var id = user.FindFirst("userId")?.Value;
        var tokens = user.FindFirst("tokens")?.Value;
        
        return Results.Json(new { id, tokens });
    }
}