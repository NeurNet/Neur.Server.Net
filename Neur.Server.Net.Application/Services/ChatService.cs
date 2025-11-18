using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
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
    public async Task<ChatEntity> CreateChatAsync(Guid userId, Guid modelId) {
        var chat = ChatEntity.Create(
            id:  Guid.NewGuid(),
            userId: userId,
            modelId: modelId,
            createdAt:  DateTime.UtcNow
        );
        await _chatsRepository.Add(chat);
        
        var savedChat = await _chatsRepository.Get(chat.Id);
        if (savedChat != null) {
            return savedChat;   
        }
        throw new Exception("Error getting the chat after create");
    }

    public async Task DeleteChatAsync(Guid userId, Guid chatId) {
        var user = await _usersRepository.GetById(userId);
        var chat = await _chatsRepository.Get(chatId);
        
        if (chat == null) {
            throw new NullReferenceException();
        }
        
        //!!Скорее всего не хорошо!!
        if (chat.User.Id == user.Id) {
            await _chatsRepository.Delete(chatId);
        }
        
        throw new NullReferenceException();
    }

    public async Task<List<MessageEntity>> GetChatMessagesAsync(Guid chatId, Guid userId) {
        var chat = await _chatsRepository.Get(chatId);
        var user = await _usersRepository.GetById(userId);
        
        if (chat == null) {
            throw new NullReferenceException();
        }

        if (chat.User.Id == user.Id) {
            var messages = await _messagesRepository.GetChatMessages(chatId);
            return messages;   
        }
        throw new NullReferenceException();
    }
    
    public async IAsyncEnumerable<string> ProcessMessageAsync(Guid userId, Guid chatId, string message) {
        var user = await _usersRepository.GetById(userId);
        var chat = await _chatsRepository.Get(chatId);
        
        await _billingService.ConsumeTokensAsync(user.Id, 1);
        
        await foreach (var chunk in _llmService.StreamOllamaResponse(chat, message)) {
            yield return chunk;
        }
    }
}