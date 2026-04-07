using System.Collections.Concurrent;
using System.Threading.Channels;
using Neur.Server.Net.Application.Exeptions;

namespace Neur.Server.Net.Application.Services.Background;

public class GenerationQueueService {
    private readonly Channel<Guid> _queue;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<Stream>> _pendingTasks;
    private readonly ConcurrentDictionary<Guid, string> _contexts;

    public GenerationQueueService() {
        _queue = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });
        _pendingTasks = new ConcurrentDictionary<Guid, TaskCompletionSource<Stream>>();
        _contexts = new ConcurrentDictionary<Guid, string>();
    }

    public async Task EnqueueAsync(Guid requestId, string context) {
        _contexts[requestId] = context;
        _pendingTasks.TryAdd(requestId, new TaskCompletionSource<Stream>());
        await _queue.Writer.WriteAsync(requestId);
        Console.WriteLine($"Добавлен запрос в очередь: {requestId}");
    }

    public string? GetContext(Guid requestId) {
        _contexts.TryGetValue(requestId, out var context);
        return context;
    }

    public async Task<Stream> WaitForResultAsync(Guid requestId, CancellationToken cancellationToken) {
        if (_pendingTasks.TryGetValue(requestId, out var tcs)) {
            Console.WriteLine("ОЖИДАНИЕ ВЫПОЛНЕНИЯ ЗАПРОСА");
            return await tcs.Task;
        }
        throw new NotFoundException();
    }

    public ChannelReader<Guid> GetEnqueueReader() => _queue.Reader;

    public void CompleteRequest(Guid requestId, Stream result) {
        Console.WriteLine("УСПЕШНОЕ ЗАВЕРШЕНИЕ");
        if (_pendingTasks.TryGetValue(requestId, out var tcs)) {
            tcs.SetResult(result);
        }
        _pendingTasks.TryRemove(requestId, out _);
        _contexts.TryRemove(requestId, out _);
    }

    public void FailRequest(Guid requestId, Exception exception) {
        Console.WriteLine("ОШИБКА, ЗАВЕРШАЮ");
        if (_pendingTasks.TryGetValue(requestId, out var tcs)) {
            tcs.SetException(exception);
        }
        _pendingTasks.TryRemove(requestId, out _);
        _contexts.TryRemove(requestId, out _);
    }
}