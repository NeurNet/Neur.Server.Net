using Neur.Server.Net.Application.Services.DTO.ChatService;
using Neur.Server.Net.Core.Entities;

namespace Neur.Server.Net.Application.Interfaces;

public interface IChatService {
    Task<ChatEntity> CreateChatAsync(Guid userId, Guid modelId, CancellationToken token = default);
    Task DeleteChatAsync(Guid chatId, Guid userId, CancellationToken token = default);
    Task<ChatWithMessagesDto> GetChatMessagesAsync(Guid chatId, Guid userId, CancellationToken token = default);
    Task<List<ChatEntity>> GetAllUserChatsAsync(Guid userId,  CancellationToken token = default);
    IAsyncEnumerable<string> ProcessPromptAsync(Guid chatId, Guid userId, string prompt, CancellationToken token);
}