namespace Neur.Server.Net.API.Contracts.Chats;

public record GenerateResponse(
    string data,
    bool? completed
);