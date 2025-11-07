using System.ComponentModel.DataAnnotations;
using Neur.Server.Net.Core.Interfaces;

namespace Neur.Server.Net.API.Contracts.Models;

public record CreateModelReqest (
    [Required] string name,
    [Required] string type,
    string? version,
    [Required] string status
);