using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Infrastructure.Clients;
using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Infrastructure.Interfaces;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services.Background;

public class GenerationService : BackgroundService, IGenerationService {
    private readonly GenerationQueueService _generationQueue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOllamaClient _ollamaClient;
    
    public GenerationService(IServiceScopeFactory scopeFactory, GenerationQueueService generationQueue, IOllamaClient ollamaClient) {
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

    public async IAsyncEnumerable<string> StreamGeneration(Guid modelId, Guid userId, Guid messageId, string prompt, CancellationToken ctsToken) {
        if (await HasPendingRequestsAsync(userId)) {
            throw new QueueException("User is already has pending requests");
        }
        
        var generationRequest = new GenerationRequestEntity(userId, modelId, 1, messageId, DateTime.UtcNow);
        
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await dbContext.GenerationRequests.AddAsync(generationRequest, ctsToken);
        await dbContext.SaveChangesAsync(ctsToken);
        
        await _generationQueue.EnqueueAsync(generationRequest.Id, prompt);
        var stream = await _generationQueue.WaitForResultAsync(generationRequest.Id, ctsToken);
        
        await foreach (var chunk in _ollamaClient.DeserializeStream(stream, ctsToken)) {
            yield return chunk;
        }
        
    }

    protected override async Task ExecuteAsync(CancellationToken ctsToken) {
        Console.WriteLine("Сервис генерации запущен!");
        while (!ctsToken.IsCancellationRequested) {
            var reader = _generationQueue.GetEnqueueReader();
            await foreach (var requestId in reader.ReadAllAsync(ctsToken)) {
                try {
                    Console.WriteLine($"НОВЫЙ ЗАПРОС: {requestId}");
                    await ProcessRequest(requestId, ctsToken);
                    Console.WriteLine("Обработано");
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error: {ex.Message}");
                    _generationQueue.FailRequest(requestId, ex);
                }
            }
            await Task.Delay(100, ctsToken);
        }
    }

    private async Task ProcessRequest(Guid requestId, CancellationToken ctsToken) {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var requestEntity = await dbContext.GenerationRequests
            .Where(x => x.Id == requestId)
            .Include(x => x.Model)
            .Include(x => x.User)
            .FirstAsync(cancellationToken: ctsToken);

        var prompt = _generationQueue.GetContext(requestId);
        if (requestEntity == null ||  prompt == null) {
            throw new NotFoundException();
        }
        
        requestEntity.StartedAt = DateTime.UtcNow;
        requestEntity.Status = RequestStatus.InProgress;
        await dbContext.SaveChangesAsync(ctsToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(ctsToken);
        
        try {
            var ollamaRequest = new OllamaRequest(requestEntity.Model.ModelName, prompt);
            var stream = await _ollamaClient.GenerateStreamAsync(ollamaRequest, ctsToken);
            
            _generationQueue.CompleteRequest(requestId, stream);
            
            requestEntity.User.Tokens--;
            requestEntity.Status = RequestStatus.Success;
            requestEntity.FinishedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(ctsToken);
            await transaction.CommitAsync(ctsToken);
        }
        
        catch (Exception ex) {
            await transaction.RollbackAsync(ctsToken);
            requestEntity.Status = RequestStatus.Failed;
            requestEntity.FinishedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(ctsToken);
        }
    }
}