namespace Neur.Server.Net.Application.Exeptions;

public class NotFoundException : Exception {
    public NotFoundException(string message = "Not found") : base(message) {
        
    }
}