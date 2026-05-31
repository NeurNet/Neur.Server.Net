using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

namespace Neur.Server.Net.Application.Interfaces;

public interface IGenerationService {
    IAsyncEnumerable<string> StreamGeneration(
        GenerationRequestEntity request,
        OllamaChatRequest chatRequest,
        Func<Task> onResponse,
        CancellationToken ctsToken);
}
