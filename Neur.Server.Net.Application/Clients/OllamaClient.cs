using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Neur.Server.Net.Application.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Application.Clients.Options;
using Neur.Server.Net.Application.Services.Contracts.OllamaService;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Application.Clients;

public class OllamaClient {
    private readonly HttpClient _httpClient;
    private readonly OllamaClientOptions _options;

    public OllamaClient(HttpClient httpClient, IOptions<OllamaClientOptions> options) {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<Stream> GenerateStreamAsync(OllamaRequest request, CancellationToken cts) {
        var requestBody = JsonSerializer.Serialize(request);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var req = new HttpRequestMessage(HttpMethod.Post, $"{_options.url}/api/generate") {
            Content = content
        };

        var response = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cts);
    }
}