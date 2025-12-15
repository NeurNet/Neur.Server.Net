namespace Neur.Server.Net.Application.Exeptions;

public class UserAccessException : Exception {
    public UserAccessException(string message) : base(message) {
        
    }
}