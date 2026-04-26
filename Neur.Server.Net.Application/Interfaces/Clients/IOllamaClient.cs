using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

namespace Neur.Server.Net.Application.Interfaces.Clients;

public interface IOllamaClient {
    Task<Stream> ChatStreamAsync(OllamaChatRequest request, CancellationToken cts);
    IAsyncEnumerable<string> DeserializeChatStream(Stream stream, CancellationToken token);
    Task<OllamaModelsResponse?> GetModelsAsync(CancellationToken token);
    Task<Stream> LoadModelAsync(string name, CancellationToken token);
}