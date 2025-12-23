namespace Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

public record OllamaRequest(
    string model,
    string prompt,
    bool stream = true
);