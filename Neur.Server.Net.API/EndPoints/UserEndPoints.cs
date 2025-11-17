using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neur.Server.Net.API.Contracts.Users;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;

namespace Neur.Server.Net.API.EndPoints;

public static class UserEndPoints {
    public static IEndpointRouteBuilder MapUserEndPoints(this IEndpointRouteBuilder app) {
        var endpoints = app.MapGroup("/api/users").WithTags("Users");
        
        endpoints.MapPost("/auth/login", Login)
            .WithSummary("Авторизация")
            .WithDescription("Сохраняет JWT токен в Cookie <b>'auth_token'</b>, также возвращает сам токен")
            .Produces<UserLoginResponse>(200)
            .Produces(401);
        endpoints.MapGet("/auth", Auth)
            .WithSummary("Аутентификация")
            .WithDescription("Проверяет <b>Cookie</b> в запросе, если секретный ключ соответствует действительному - возвращает пользователя")
            .Produces<UserAuthResponse>(200, "application/json")
            .Produces(401)
            .RequireAuthorization();
        endpoints.MapPost("/auth/logout", Logout)
            .WithSummary("Выход из сервиса")
            .WithDescription("Удаляет Cookie <b>'auth_token'</b>");
        endpoints.MapGet("/tokens/", GetTokens)
            .WithSummary("Получить количество токенов пользоваетеля");
        
        return endpoints;
    }
    
    private static async Task<IResult> Login(UserLoginRequest req, UserService userService, HttpResponse response, IOptions<JwtOptions> _jwtOptions) {
        try {
            var jwtOptions = _jwtOptions.Value;
            var token = await userService.Login(req.username, req.password);
            response.Cookies.Append("auth_token", token, new CookieOptions {
                HttpOnly = true,
                Secure = true, // если HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddHours(jwtOptions.ExpiresHours)
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
        var username = user.FindFirst("username")?.Value;
        
        return Results.Json(new UserAuthResponse(id, username));
    }

    private static async Task<IResult> GetTokens([FromQuery] Guid userId, IUsersRepository repository) {
        var user = await repository.GetById(userId);
        return Results.Json(new UserTokensResponse(user.Id, user.Tokens));
    }
}