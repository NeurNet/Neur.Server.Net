using Neur.Server.Net.Application.Exeptions;

namespace Neur.Server.Net.Application.Exceptions;

public class NotAuthorizedException : BaseException {
    public NotAuthorizedException(string message = "Not authorized exception") : base(401, message) {
    }
}