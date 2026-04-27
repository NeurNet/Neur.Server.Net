using System.Text.Json.Serialization;

namespace Neur.Server.Net.Application.DTOs.Chat;

public enum GenerationChunkResponseType {
    Meta = 0,
    Data = 1
}

public record GenerationChunkResponse(
    [property: JsonPropertyName("type")] GenerationChunkResponseType Type,
    [property: JsonPropertyName("data")] string Data,
    [property: JsonPropertyName("completed")] bool IsCompleted
);