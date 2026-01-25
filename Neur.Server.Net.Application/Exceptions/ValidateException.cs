using Neur.Server.Net.Application.Exeptions;

namespace Neur.Server.Net.Application.Exceptions;

public class ValidateException : BaseException {
    public ValidateException(string message) : base(400, message) {
    }
}