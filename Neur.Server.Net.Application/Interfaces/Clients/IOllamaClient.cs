using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

namespace Neur.Server.Net.Application.Interfaces.Clients;

public interface IOllamaClient {
    Task<Stream> GenerateStreamAsync(OllamaGenerationRequest generationRequest, CancellationToken cts);
    public IAsyncEnumerable<string> DeserializeStream(Stream stream, CancellationToken token);
    public Task<OllamaModelsResponse?> GetOllamaModels(CancellationToken token);
}