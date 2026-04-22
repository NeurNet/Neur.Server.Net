using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure.Clients;
using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Infrastructure.Interfaces;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services.Background;

public class GenerationService : BackgroundService {
    private readonly GenerationQueueService _generationQueue;
    private readonly IOllamaClient _ollamaClient;
    private readonly ILogger<GenerationService> _logger;
    
    public GenerationService(GenerationQueueService generationQueue, IOllamaClient ollamaClient, ILogger<GenerationService> logger) {
        _generationQueue = generationQueue;
        _ollamaClient = ollamaClient;
        _logger = logger;
    }

    public async IAsyncEnumerable<string> StreamGeneration(GenerationRequestEntity request, string prompt, Func<Task> onResponse, CancellationToken ctsToken) {
        await _generationQueue.EnqueueAsync(request, prompt);
        var stream = await _generationQueue.WaitForResultAsync(request.UserId, ctsToken);
        await onResponse();
        await foreach (var chunk in _ollamaClient.DeserializeStream(stream, ctsToken)) {
            yield return chunk;
        }
        _generationQueue.CompleteRequest(request.UserId);
    }

    protected override async Task ExecuteAsync(CancellationToken ctsToken) {
        _logger.LogInformation("Generation Service is running!");
        while (!ctsToken.IsCancellationRequested) {
            var reader = _generationQueue.GetEnqueueReader();
            await foreach (var request in reader.ReadAllAsync(ctsToken)) {
                try {
                    _logger.LogInformation("Processing generation request {requestId}", request.Id);
                    await ProcessRequest(request, ctsToken);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Generation request could not be processed.");
                    _generationQueue.FailRequest(request.UserId, ex);
                }
            }
            await Task.Delay(100, ctsToken);
        }
    }

    private async Task ProcessRequest(GenerationRequestEntity request, CancellationToken ctsToken) {
        var prompt = _generationQueue.GetContext(request.UserId);
        if (prompt == null) {
            throw new NotFoundException();
        }
        var ollamaRequest = new OllamaGenerationRequest(request.Model.ModelName, prompt);
        var stream = await _ollamaClient.GenerateStreamAsync(ollamaRequest, ctsToken);
        _generationQueue.GiveResult(request.UserId, stream);
    }
}