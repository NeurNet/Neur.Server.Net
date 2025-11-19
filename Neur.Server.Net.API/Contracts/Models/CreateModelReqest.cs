using System.ComponentModel.DataAnnotations;

namespace Neur.Server.Net.API.Contracts.Models;

public record CreateModelReqest (
    [Required] string name,
    [Required] string model,
    [Required] string type,
    string? version,
    [Required] string status
);