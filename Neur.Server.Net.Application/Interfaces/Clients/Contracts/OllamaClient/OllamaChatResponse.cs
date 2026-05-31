using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;

namespace Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

public record OllamaChatResponse(OllamaChatMessage message, bool done);
