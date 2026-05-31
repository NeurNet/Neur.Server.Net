using System.ComponentModel.DataAnnotations;

namespace Neur.Server.Net.API.Contracts.Users;

public record UserLoginRequest (
    [Required] string username,
    [Required] string password
);