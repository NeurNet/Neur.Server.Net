using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Neur.Server.Net.API.Contracts.Users;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;

namespace Neur.Server.Net.API.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : Controller {
    private IUserRepository _userRepository;
    
    public UserController(IUserRepository userRepository) {
        _userRepository = userRepository;
    }

    [HttpPost("auth")]
    public async Task<IResult> Login(UserLoginRequest req, UserService userService) {
        try {
            var token = await userService.Login(req.username, req.password);
            Response.Cookies.Append("auth_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // если HTTPS
                SameSite = SameSiteMode.Strict
            });
            
            return Results.Ok();
        }
        catch (Exception ex) {
            return Results.BadRequest(ex.Message);
        }
    }
    
    [HttpGet("auth")]
    [Authorize]
    public async Task<IResult> Auth(ClaimsPrincipal user) {
        var id = user.FindFirst("id")?.Value;
        var tokens = user.FindFirst("tokens")?.Value;
        
        return Results.Ok(new { id, tokens });
    }
}