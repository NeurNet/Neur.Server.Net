using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Neur.Server.Net.Application.Services.Contracts.OllamaService;
using Neur.Server.Net.Application.Services.Options;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Application.Services;

public class OllamaService {
    private readonly HttpClient _httpClient;
    private readonly OllamaServiceOptions _options;
    private readonly IRequestsRepository _requestsRepository;

    public OllamaService(HttpClient httpClient, IOptions<OllamaServiceOptions> options, IRequestsRepository repository) {
        _httpClient = httpClient;
        _options = options.Value;
        _requestsRepository = repository;
    }

    public async IAsyncEnumerable<string> StreamResponse(OllamaRequest request) {
        var requestBody = JsonSerializer.Serialize(request);
        var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

        var req = new HttpRequestMessage(HttpMethod.Post, $"{_options.url}/api/generate")
        {
            Content = content
        };

        using var response = await _httpClient.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(responseStream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;

            if (root.TryGetProperty("response", out var chunk))
                yield return chunk.GetString()!;

            if (root.TryGetProperty("done", out var done) && done.GetBoolean())
                yield break;
        }
    }

    public async Task<GenerateResponse?> ProcessPrompt(ChatEntity chat, string prompt) {
        var request = RequestEntity.Create(
            chat.Id,
            prompt,
            DateTime.UtcNow
        );
        var reqId = await _requestsRepository.Add(request);
        
        var requestBody = JsonSerializer.Serialize(request);
        var stringContent = new StringContent(requestBody, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"{_options.url}/api/generate", stringContent);
        
        if (response.IsSuccessStatusCode) {
            var responseString = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OllamaResponse>(responseString);
            if (result != null) {
                await _requestsRepository.Update(reqId, result.response, DateTime.UtcNow);
            
                return new GenerateResponse(
                    StartedAt: DateTime.UtcNow, 
                    FinishedAt: DateTime.UtcNow,
                    Prompt: prompt,
                    Result: result.response,
                    Model: result.model
                );
            }
        }

        return null;
    }
}