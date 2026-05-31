using Neur.Server.Net.Application.Exeptions;

namespace Neur.Server.Net.Application.Exceptions;

public class ModelDeletedException : BaseException {
    public ModelDeletedException(string message = "The model was deleted") : base(410, message) {
        
    }
}