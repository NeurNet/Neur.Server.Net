namespace Neur.Server.Net.Application.Interfaces;

public interface IGenerationService {
    IAsyncEnumerable<string> StreamGeneration(Guid modelId, Guid userId, Guid messageId, string prompt, CancellationToken ctsToken);
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
    void Dispose();
    Task? ExecuteTask { get; }
}