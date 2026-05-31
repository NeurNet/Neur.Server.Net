using System.Text.Json.Serialization;

namespace Neur.Server.Net.API.Contracts;

public record ServerErrorResponse(
    [property: JsonPropertyName("status")] int Status,
    [property: JsonPropertyName("error")] string Error
);