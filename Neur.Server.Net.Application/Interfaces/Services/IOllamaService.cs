using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;

namespace Neur.Server.Net.Application.Interfaces.Services;

public interface IOllamaService {
    public Task<List<OllamaModel>> GetOllamaModels(CancellationToken token);
}