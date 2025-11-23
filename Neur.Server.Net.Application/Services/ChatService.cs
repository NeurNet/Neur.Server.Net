using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services;

public class ChatService {
    private readonly BillingService _billingService;
    private readonly LLMService _llmService;
    private readonly ApplicationDbContext _dbContext;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IChatsRepository _chatsRepository;
    public ChatService(BillingService billingService, LLMService llmService, ApplicationDbContext dbContext, IChatsRepository chatsRepository, IMessagesRepository messagesRepository, IUsersRepository usersRepository) {
        _billingService = billingService;
        _llmService = llmService;
        _dbContext = dbContext;
        _chatsRepository = chatsRepository;
        _messagesRepository = messagesRepository;
        _usersRepository = usersRepository;
    }
    public async Task<ChatEntity> CreateChatAsync(Guid userId, Guid modelId) {
        var chat = ChatEntity.Create(
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
        else {
            throw new NullReferenceException();   
        }
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
        else {
            throw new NullReferenceException();   
        }
    }
    

    public async IAsyncEnumerable<string> ProcessMessageAsync(Guid userId, Guid chatId, string message) {
        var user = await _dbContext.Users.FindAsync(userId) ?? throw new NullReferenceException();
        var chat = await _chatsRepository.Get(chatId) ??  throw new NullReferenceException();

        if (user.Tokens <= 0) {
            throw new BillingException("User has no tokens");
        }
        
        var stream = _llmService.StreamOllamaResponse(chat, message).GetAsyncEnumerator();
        
        if (!await stream.MoveNextAsync())
            yield break; 

        var firstChunk = stream.Current;
        
        using (var tx = await _dbContext.Database.BeginTransactionAsync())
        {
            user.ConsumeTokens(1);
            await _dbContext.SaveChangesAsync();
            await tx.CommitAsync();
        }
        
        yield return firstChunk;
        
        while (await stream.MoveNextAsync())
            yield return stream.Current;
    }
}