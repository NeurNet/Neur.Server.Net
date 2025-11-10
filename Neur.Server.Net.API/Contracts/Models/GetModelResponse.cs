namespace Neur.Server.Net.API.Contracts.Models;

public record GetModelResponse (
    Guid id,
    string name,
    string model,
    string version,
    string status,
    DateTime createdAt,
    DateTime? updatedAt
);