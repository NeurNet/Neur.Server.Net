namespace Neur.Server.Net.Application.Clients.Contracts.OllamaClient;

public record OllamaRequest(
    string model,
    string prompt,
    bool stream
);