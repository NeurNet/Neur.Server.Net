using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;

namespace Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

public record OllamaChatRequest(
    string model,
    List<OllamaChatMessage> messages,
    bool stream = true
);
