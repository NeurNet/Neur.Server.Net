namespace Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

public record OllamaResponse (
    string model,
    string response,
    bool done
);