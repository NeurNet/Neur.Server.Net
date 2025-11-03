using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.Users;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.API.Controllers;

[Route("api/users")]
[ApiController]
public class UserController : Controller {
    private IUserRepository _userRepository;
    
    public UserController(IUserRepository userRepository) {
        _userRepository = userRepository;
    }

    [HttpPost]
    public async Task<IResult> Login(CreateUserRequest req, UserService userService) {
        try {
            var token = await userService.Login(req.username, req.password);
            return Results.Ok(token);
        }
        catch (Exception ex) {
            return Results.BadRequest(ex.Message);
        }
    }
}