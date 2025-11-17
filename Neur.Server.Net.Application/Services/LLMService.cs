using Neur.Server.Net.Application.Clients;
using Neur.Server.Net.Application.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Application.Services.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Application.Services;

public class LLMService {
    private readonly OllamaClient _client;
    private readonly IRequestsRepository _requestsRepository;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public LLMService(OllamaClient client, IRequestsRepository repository) {
        _client = client;
        _requestsRepository = repository;
    }

    private async IAsyncEnumerable<string> StreamOllamaResponse(RequestEntity request) {
        await _semaphore.WaitAsync();
        try {
            var ollamaRequest = new OllamaRequest(request.Chat.Model.ModelName, request.Prompt, true);
            var requestResponse = "";
            request.StartedAt = DateTime.UtcNow;
            await _requestsRepository.Update(request);
            await foreach (var response in _client.StreamResponse(ollamaRequest)) {
                requestResponse += response;
                yield return response;
            }
            request.FinishedAt = DateTime.UtcNow;
            await _requestsRepository.Update(request);
        }
        finally {
            _semaphore.Release();
        }
    }
    
    public LLMSession GetSession(RequestEntity request) {
        return new LLMSession(() => StreamOllamaResponse(request), _semaphore);
    }
}