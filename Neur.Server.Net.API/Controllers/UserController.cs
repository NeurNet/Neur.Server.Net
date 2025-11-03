using Microsoft.AspNetCore.Mvc;
using Neur.Server.Net.API.Contracts.Users;
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
    public async Task<IActionResult> AddUser(CreateUserRequest req) {
        try {
            var user = UserEntity.Create(
                Guid.NewGuid(),
                req.username,
                name: "",
                ""
            );
            await _userRepository.Add(user);
            return Ok();
        }
        catch (Exception e) {
            return BadRequest(e.Message);
        }
    }
}