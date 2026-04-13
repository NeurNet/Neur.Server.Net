namespace Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

public record OllamaGenerationRequest(
    string model,
    string prompt,
    bool stream = true
);