using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;

namespace Neur.Server.Net.Application.Interfaces.Services;

public interface IOllamaService {
    public Task<List<OllamaModel>> GetOllamaModelsAsync(CancellationToken token);
    public IAsyncEnumerable<OllamaLoadModelResponse> LoadModelAsync(string name, CancellationToken token);
}