using Neur.Server.Net.Application.Services.DTO.ChatService;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Interfaces;

public interface IChatService {
    Task<ChatEntity> CreateChatAsync(Guid userId, Guid modelId);
    Task DeleteChatAsync(Guid chatId, Guid userId);
    Task<IEnumerable<ChatWithMessagesDto>> GetChatMessagesAsync(Guid chatId, Guid userId);
    Task<List<ChatEntity>> GetAllUserChats(Guid userId);
    IAsyncEnumerable<string> ProcessPromptAsync(Guid chatId, Guid userId, string prompt);
}