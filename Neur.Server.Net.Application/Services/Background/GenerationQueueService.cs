using System.Collections.Concurrent;
using System.Threading.Channels;
using Neur.Server.Net.Application.Exeptions;

namespace Neur.Server.Net.Application.Services.Background;

public class GenerationQueueService {
    private readonly Channel<Guid> _queue;
    private ConcurrentDictionary<Guid, TaskCompletionSource<Stream>> _pendingTasks;
    
    public GenerationQueueService() {
        _queue = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false });
        _pendingTasks = new ConcurrentDictionary<Guid, TaskCompletionSource<Stream>>();
    }

    public async Task EnqueueAsync(Guid requestId) {
        _pendingTasks.TryAdd(requestId, new TaskCompletionSource<Stream>());
        await _queue.Writer.WriteAsync(requestId);
        Console.WriteLine($"Добавлен запрос в очередь: {requestId}");
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
    }

    public void FailRequest(Guid requestId, Exception exception) {
        Console.WriteLine("ОШИБКА, ЗАВЕРШАЮ");
        if (_pendingTasks.TryGetValue(requestId, out var tcs)) {
            tcs.SetException(exception);
        }
    }
}