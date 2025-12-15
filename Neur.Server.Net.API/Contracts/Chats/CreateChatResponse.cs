namespace Neur.Server.Net.API.Contracts.Chats;

public record CreateChatResponse( 
    Guid chatId,
    Guid modelId
);