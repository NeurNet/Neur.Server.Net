using System.Text.Json.Serialization;

namespace Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;

public record OllamaModel(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("size")] long Size,
    [property: JsonPropertyName("details")] OllamaModelDetails Details);

public record OllamaModelDetails(
    [property: JsonPropertyName("family")] string Family,
    [property: JsonPropertyName("parameter_size")] string ParameterSize,
    [property: JsonPropertyName("quantization_level")] string QuantizationLevel);

public record OllamaModelsResponse(
    [property: JsonPropertyName("models")] List<OllamaModel> Models);