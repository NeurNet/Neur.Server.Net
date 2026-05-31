using System.ComponentModel.DataAnnotations;

namespace Neur.Server.Net.API.Contracts.Models;

public record CreateModelReqest (
    [Required] string name,
    [Required] string model,
    [Required] string context,
    [Required] string type,
    [Required] string version,
    [Required] string status
);