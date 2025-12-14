namespace Neur.Server.Net.Application.Exeptions;

public class QueueException : Exception {
    public QueueException(string message = "Queue exception") : base(message) {
        
    }
}