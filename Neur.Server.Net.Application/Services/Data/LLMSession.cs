using Neur.Server.Net.Application.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Application.Services.Contracts.OllamaService;

namespace Neur.Server.Net.Application.Services.Data;

public class LLMSession {
    private readonly Func<IAsyncEnumerable<string>> _executor;
    private readonly SemaphoreSlim _semaphore;
    
    public LLMSession(Func<IAsyncEnumerable<string>> response, SemaphoreSlim semaphore) {
        _executor = response;
        _semaphore = semaphore;
    }

    public IAsyncEnumerable<string> StreamResponse() {
        return _executor();
    }
}