using System.ComponentModel.DataAnnotations;

namespace Neur.Server.Net.API.Contracts.Users;

public record CreateUserRequest (
    [Required] string username,
    [Required] string password
);