namespace Neur.Server.Net.Application.Exeptions;

public class BillingException : BaseException {
    public BillingException(string message = "Billing exception") : base(402, message) {
        
    }
}