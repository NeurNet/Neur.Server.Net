using System.ComponentModel.DataAnnotations;

namespace Neur.Server.Net.API.Contracts.Chats;

public record GenerateRequest(
    [Required] string prompt
);