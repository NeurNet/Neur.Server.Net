using System.Security.Claims;
using Neur.Server.Net.API.Contracts.Users;
using Neur.Server.Net.Application.Services;

namespace Neur.Server.Net.API.EndPoints;

public static class UserEndPoints {
    public static IEndpointRouteBuilder MapUserEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/users/auth");
        
        endpoints.MapPost("/login", Login)
            .WithSummary("Авторизация")
            .WithDescription("Сохраняет JWT токен в Cookie <b>'auth_token'</b>, также возвращает сам токен")
            .Produces<UserLoginResponse>(200)
            .Produces(401);
        endpoints.MapPost("/logout", Logout)
            .WithSummary("Выход из сервиса")
            .WithDescription("Удаляет Cookie <b>'auth_token'</b>");
        endpoints.MapGet(String.Empty, Auth)
            .WithSummary("Аутентификация")
            .WithDescription("Проверяет <b>Cookie</b> в запросе, если секретный ключ соответствует действительному - возвращает пользователя")
            .Produces<UserAuthResponse>(200, "application/json")
            .Produces(401)
            .RequireAuthorization();
        
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

            return Results.Ok(new UserLoginResponse(token));
        }
        catch (NullReferenceException) {
            return Results.Unauthorized();
        }
        catch (Exception ex) {
            return Results.BadRequest(ex.Message);
        }
    }

    private static async Task<IResult> Logout(ClaimsPrincipal user, HttpResponse response) {
        response.Cookies.Delete("auth_token");
        return Results.Ok();
    }
    
    private static async Task<IResult> Auth(ClaimsPrincipal user) {
        var id = user.FindFirst("userId")?.Value;
        var tokens = user.FindFirst("tokens")?.Value;
        
        return Results.Json(new { id, tokens });
    }
}