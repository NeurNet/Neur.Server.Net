namespace Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

public record OllamaGenerationResponse (
    string model,
    string response,
    bool done
);