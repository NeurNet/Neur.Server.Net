using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Application.Interfaces.Services;

namespace Neur.Server.Net.Infrastructure.Services;

public class OllamaService : IOllamaService {
    private readonly IOllamaClient _client; 
        
    public OllamaService(IOllamaClient client) {
        _client = client;
    }
    
    public async Task<List<OllamaModel>> GetOllamaModels(CancellationToken token) {
        var response = await _client.GetOllamaModels(token);
        return response?.Models ?? [];
    }
}