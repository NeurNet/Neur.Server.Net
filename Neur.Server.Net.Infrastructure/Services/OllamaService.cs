using System.Runtime.CompilerServices;
using System.Text.Json;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Application.Interfaces.Services;

namespace Neur.Server.Net.Infrastructure.Services;

public class OllamaService : IOllamaService {
    private readonly IOllamaClient _client; 
        
    public OllamaService(IOllamaClient client) {
        _client = client;
    }
    
    public async Task<List<OllamaModel>> GetOllamaModelsAsync(CancellationToken token) {
        var response = await _client.GetModelsAsync(token);
        return response?.Models ?? [];
    }

    public async IAsyncEnumerable<OllamaLoadModelResponse> LoadModelAsync(string name, [EnumeratorCancellation] CancellationToken token) {
        var responseStream = await _client.LoadModelAsync(name, token);
        using var reader = new StreamReader(responseStream);
        
        var ctsResponse = new CancellationTokenSource();
        
        while (!reader.EndOfStream && !token.IsCancellationRequested) {
            ctsResponse.CancelAfter(TimeSpan.FromSeconds(30));
            
            var line = await reader.ReadLineAsync(ctsResponse.Token);
            if (string.IsNullOrWhiteSpace(line)) continue;

            OllamaLoadModelResponse? content;
            try {
                content = JsonSerializer.Deserialize<OllamaLoadModelResponse>(line);
            }
            catch (JsonException) {
                continue;
            }

            if (content == null) continue;
            yield return content;
        }
    }
}