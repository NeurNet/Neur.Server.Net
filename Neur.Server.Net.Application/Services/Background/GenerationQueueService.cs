using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Services.Background;

public class GenerationQueueService {
    private readonly Channel<GenerationRequestEntity> _queue;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Stream>> _pendingTasks;
    private readonly ConcurrentDictionary<Guid, string> _contexts;
    private readonly ILogger<GenerationQueueService> _logger;

    public GenerationQueueService(ILogger<GenerationQueueService> logger) {
        _queue = Channel.CreateUnbounded<GenerationRequestEntity>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });
        _pendingTasks = new ConcurrentDictionary<Guid, TaskCompletionSource<Stream>>();
        _contexts = new ConcurrentDictionary<Guid, string>();
        _logger = logger;
    }

    public async Task EnqueueAsync(GenerationRequestEntity request, string context) {
        if (!_pendingTasks.TryAdd(request.UserId, new TaskCompletionSource<Stream>())) {
            throw new QueueException("User already in queue");
        };
        _contexts[request.UserId] = context;
        await _queue.Writer.WriteAsync(request);
        _logger.LogInformation("Queued generation request");
    }

    public string? GetContext(Guid userId) {
        _contexts.TryGetValue(userId, out var context);
        return context;
    }

    public async Task<Stream> WaitForResultAsync(Guid userId, CancellationToken cancellationToken) {
        if (_pendingTasks.TryGetValue(userId, out var tcs)) {
            _logger.LogInformation("Waiting for request processing...");
            return await tcs.Task.WaitAsync(cancellationToken);
        }
        throw new NotFoundException();
    }

    public ChannelReader<GenerationRequestEntity> GetEnqueueReader() => _queue.Reader;

    public void GiveResult(Guid userId, Stream result) {
        _logger.LogInformation("Return generation output stream");
        if (_pendingTasks.TryGetValue(userId, out var tcs)) {
            tcs.SetResult(result);
        }
    }

    public void CompleteRequest(Guid userId) {
        _pendingTasks.TryRemove(userId, out _);
        _contexts.TryRemove(userId, out _);
    }

    public void FailRequest(Guid userId, Exception exception) {
        _logger.LogError(exception, "The request was aborted with an error");
        if (_pendingTasks.TryGetValue(userId, out var tcs)) {
            tcs.SetException(exception);
        }
        _pendingTasks.TryRemove(userId, out _);
        _contexts.TryRemove(userId, out _);
    }
}