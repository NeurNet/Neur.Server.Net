using Neur.Server.Net.API.Contracts.GenerationRequests;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.API.Extensions;

public static class RequestsExtenstion {
    public static GenerationRequestResponse ToResponse(this (IEnumerable<GenerationRequestEntity> requests, int count) source) {
        var items = source.requests.Select(x =>
            new GenerationRequestResponseItem(
                x.Id, x.ModelId, x.ModelName, x.ModelOllama, x.TokenCost, x.Status, x.CreatedAt,
                x.StartedAt, x.FinishedAt, 
                new GenerationRequestUserResponse(x.User.Id, x.User.Username, x.User.Name, x.User.Surname),
                x.ResponseMessage != null ? new GenerationRequestMessageResponse(x.ResponseMessageId!.Value, x.ResponseMessage.Role, x.ResponseMessage.Content) : null
            )
        );
        return new GenerationRequestResponse(items, source.count);
    }
}