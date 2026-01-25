namespace Neur.Server.Net.Core.Exeptions;

public class CreatingEntityException : Exception {
    public CreatingEntityException(string message = "Cannot create a entity") : base(message) {
        
    }
}