namespace Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;

using System.Text.Json.Serialization;

public record OllamaLoadModelResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("digest"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Digest,
    [property: JsonPropertyName("total"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? Total,
    [property: JsonPropertyName("completed"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] long? Completed,
    [property: JsonPropertyName("error"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Error
);