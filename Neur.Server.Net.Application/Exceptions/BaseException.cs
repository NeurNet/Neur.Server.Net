namespace Neur.Server.Net.Application.Exeptions;

public class BaseException : Exception {
    public readonly int StatusCode;
    public BaseException(int code, string message) : base(message) {
        StatusCode = code;
    }
}