using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

namespace Neur.Server.Net.Application.Interfaces.Clients;

public interface IOllamaClient {
    Task<Stream> GenerateStreamAsync(OllamaRequest request, CancellationToken cts);
    public IAsyncEnumerable<string> DeserializeStream(Stream stream, CancellationToken token);
}