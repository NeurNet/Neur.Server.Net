using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.API.Contracts.Models;

public record GetModelResponse (
    Guid id,
    string name,
    string model,
    ModelType type,
    string version,
    string status,
    DateTime createdAt,
    DateTime? updatedAt
);