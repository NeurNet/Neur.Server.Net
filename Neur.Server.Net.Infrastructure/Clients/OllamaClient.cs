using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Application.Options;
using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Infrastructure.Interfaces;

namespace Neur.Server.Net.Infrastructure.Clients;

public class OllamaClient : IOllamaClient {
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaClient> _logger;
    private volatile string _url = string.Empty;

    public OllamaClient(HttpClient httpClient, ILogger<OllamaClient> logger) {
        _httpClient = httpClient;
        _logger = logger;
    }

    public void SetOptions(OllamaSettingsOptions options) {
        _url = options.Url;
        _logger.LogInformation("OllamaClient url updated: {url}", _url);
    }

    public async Task<Stream> ChatStreamAsync(OllamaChatRequest request, CancellationToken cts) {
        var requestBody = JsonSerializer.Serialize(request);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var req = new HttpRequestMessage(HttpMethod.Post, $"{_url}/api/chat") {
            Content = content
        };

        var response = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cts);
    }

    public async IAsyncEnumerable<string> DeserializeChatStream(Stream stream, CancellationToken token) {
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream && !token.IsCancellationRequested) {
            var line = await reader.ReadLineAsync();
            if (line == null) continue;
            var chunk = JsonSerializer.Deserialize<OllamaChatResponse>(line);
            if (chunk == null) continue;

            yield return chunk.message.content;
        }
    }

    public async Task<OllamaModelsResponse?> GetModelsAsync(CancellationToken token) {
        var response = await _httpClient.GetAsync($"{_url}/api/tags", token);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(token);
        return JsonSerializer.Deserialize<OllamaModelsResponse>(responseBody);
    }

    public async Task<Stream> LoadModelAsync(string name, CancellationToken token) {
        var request = new OllamaLoadModelRequest(name, true);
        var requestBody = JsonSerializer.Serialize(request);
        var stringContent = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{_url}/api/pull", stringContent, token);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }
}
