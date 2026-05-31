namespace Neur.Server.Net.Application.Exeptions;

public class QueueException : BaseException {
    public QueueException(string message = "Queue exception") : base(400, message) {
        
    }
}