namespace Neur.Server.Net.Application.Exeptions;

public class NotFoundException : BaseException {
    public NotFoundException(string message = "Not found") : base(404, message) {
        
    }
}