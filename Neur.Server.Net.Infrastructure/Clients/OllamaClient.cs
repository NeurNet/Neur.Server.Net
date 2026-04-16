using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Neur.Server.Net.Application.Clients.Options;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Infrastructure.Interfaces;

namespace Neur.Server.Net.Infrastructure.Clients;

/// <summary>
/// The client object to send requests to Ollama
/// </summary>
public class OllamaClient : IOllamaClient {
    private readonly HttpClient _httpClient;
    private readonly OllamaClientOptions _options;

    /// <summary>
    /// Initializes a new <see cref="OllamaClient"/> class
    /// </summary>
    /// <param name="httpClient">The HTTP client for API requests</param>
    /// <param name="options">Configuration options for the client</param>
    public OllamaClient(HttpClient httpClient, IOptions<OllamaClientOptions> options) {
        _httpClient = httpClient;
        _options = options.Value;
    }
    
    /// <summary>
    /// The method of sending a generation generationRequest to Ollama
    /// </summary>
    /// <param name="generationRequest">Request object</param>
    /// <param name="cts">Cancellation token</param>
    /// <returns></returns>
    public async Task<Stream> GenerateStreamAsync(OllamaGenerationRequest generationRequest, CancellationToken cts) {
        var requestBody = JsonSerializer.Serialize(generationRequest);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var req = new HttpRequestMessage(HttpMethod.Post, $"{_options.url}/api/generate") {
            Content = content
        };

        var response = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync(cts);
    }
    
    public async IAsyncEnumerable<string> DeserializeStream(Stream stream, CancellationToken token) {
        using var reader = new StreamReader(stream);
        while (!reader.EndOfStream && !token.IsCancellationRequested) {
            var line = await reader.ReadLineAsync();
            if (line == null) continue;
            var content = JsonSerializer.Deserialize<OllamaGenerationResponse>(line);
            if (content == null) continue;
            
            yield return content.response;
        }
    }

    public async Task<OllamaModelsResponse?> GetModelsAsync(CancellationToken token) {
        var response = await _httpClient.GetAsync($"{_options.url}/api/tags", token);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(token);
        var result = JsonSerializer.Deserialize<OllamaModelsResponse>(responseBody);
        return result;
    }

    public async Task<Stream> LoadModelAsync(string name, CancellationToken token) {
        var request = new OllamaLoadModelRequest(name, true);
        var requestBody = JsonSerializer.Serialize(request);
        var stringContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_options.url}/api/pull", stringContent, token);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStreamAsync();
    }
}