using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neur.Server.Net.Application.Clients;
using Neur.Server.Net.Application.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services.Background;

public class GenerationService : BackgroundService {
    private readonly GenerationQueueService _generationQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OllamaClient _ollamaClient;
    
    public GenerationService(IServiceScopeFactory scopeFactory, GenerationQueueService generationQueue, OllamaClient ollamaClient) {
        _generationQueue = generationQueue;
        _scopeFactory = scopeFactory;
        _ollamaClient = ollamaClient;
    }
    
    private async Task<bool> HasPendingRequestsAsync(Guid userId) {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var request = await dbContext.GenerationRequests
            .AsNoTracking()
            .Where(x => x.UserId == userId && (x.Status == RequestStatus.Pending ||  x.Status == RequestStatus.InProgress))
            .FirstOrDefaultAsync();
        return request != null;
    }

    public async Task<Stream> StreamGeneration(Guid modelId, Guid userId, string context, CancellationToken cancellationToken) {
        if (await HasPendingRequestsAsync(userId)) {
            throw new QueueException("User is already has pending requests");
        }
        
        var generationRequest = new GenerationRequestEntity(userId, modelId, 1, context, DateTime.UtcNow);
        
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await dbContext.GenerationRequests.AddAsync(generationRequest);
        await dbContext.SaveChangesAsync();
        
        await _generationQueue.EnqueueAsync(generationRequest.Id);
        var stream = await _generationQueue.WaitForResultAsync(generationRequest.Id, cancellationToken);
        
        return stream;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        Console.WriteLine("Сервис генерации запущен!");
        while (!stoppingToken.IsCancellationRequested) {
            var reader = _generationQueue.GetEnqueueReader();
            await foreach (var requestId in reader.ReadAllAsync(stoppingToken)) {
                try {
                    Console.WriteLine($"НОВЫЙ ЗАПРОС: {requestId}");
                    await ProcessRequest(requestId, stoppingToken);
                    Console.WriteLine("Обработано");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error: {ex.Message}");
                    _generationQueue.FailRequest(requestId, ex);
                }
            }
            await Task.Delay(100, stoppingToken);
        }
    }

    private async Task ProcessRequest(Guid requestId, CancellationToken stoppingToken) {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var requestEntity = await dbContext.GenerationRequests
            .Where(x => x.Id == requestId)
            .Include(x => x.Model)
            .Include(x => x.User)
            .FirstAsync(cancellationToken: stoppingToken);

        if (requestEntity == null) {
            throw new NotFoundException("Request not found");
        }

        requestEntity.StartedAt = DateTime.UtcNow;
        requestEntity.Status = RequestStatus.InProgress;
        await dbContext.SaveChangesAsync(stoppingToken);

        try {
            var ollamaRequest = new OllamaRequest(requestEntity.Model.ModelName, requestEntity.Prompt);
            var stream = await _ollamaClient.GenerateStreamAsync(ollamaRequest, stoppingToken);
            requestEntity.User.Tokens--;
            requestEntity.Status = RequestStatus.Success;
            requestEntity.FinishedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(stoppingToken);
            _generationQueue.CompleteRequest(requestId, stream);
        }
        
        catch (Exception ex) {
            requestEntity.Status = RequestStatus.Failed;
            requestEntity.FinishedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }
}