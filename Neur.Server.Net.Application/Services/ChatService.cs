using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;

namespace Neur.Server.Net.Application.Services;

public class ChatService {
    private readonly BillingService _billingService;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IChatsRepository _chatsRepository;
    private readonly LLMService _llmService;
    public ChatService(BillingService billingService, IChatsRepository chatsRepository, IMessagesRepository messagesRepository, IUsersRepository usersRepository, LLMService llmService) {
        _billingService = billingService;
        _chatsRepository = chatsRepository;
        _messagesRepository = messagesRepository;
        _usersRepository = usersRepository;
        _llmService = llmService;
    }
    
    public async IAsyncEnumerable<string> ProcessMessage(Guid userId, Guid chatId, string message) {
        var user = await _usersRepository.GetById(userId);
        var chat = await _chatsRepository.Get(chatId);
        
        await _billingService.ConsumeTokensAsync(user.Id, 1);
        await foreach (var chunk in _llmService.StreamOllamaResponse(chat, message)) {
            yield return chunk;
        }
    }
}