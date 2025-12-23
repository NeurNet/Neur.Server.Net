using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

namespace Neur.Server.Net.Infrastructure.Interfaces;

public interface IOllamaClient {
    Task<Stream> GenerateStreamAsync(OllamaRequest request, CancellationToken cts);
}