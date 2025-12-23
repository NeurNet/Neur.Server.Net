using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Neur.Server.Net.Application.Clients;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Extensions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Services.Background;
using Neur.Server.Net.Application.Services.DTO.ChatService;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;
using Neur.Server.Net.Infrastructure.Clients;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services;

public class ChatService : IChatService {
    private readonly ApplicationDbContext _dbContext;
    private readonly GenerationService _generationService;
    private readonly IMessageService _messageService;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IChatsRepository _chatsRepository;
    public ChatService(ApplicationDbContext dbContext, GenerationService generationService,
        IChatsRepository chatsRepository, IMessagesRepository messagesRepository, IMessageService messageService) {
        _dbContext = dbContext;
        _generationService = generationService;
        _chatsRepository = chatsRepository;
        _messagesRepository = messagesRepository;
        _messageService = messageService;
    }
    private async Task<string> ReadContext(Guid chatId, string currentMessage, string baseContext) {
        List<MessageEntity> messages = await _messagesRepository.GetChatMessages(chatId);
        var contextManager = new ContextManager();
        
        contextManager.AddBaseContext(baseContext);
        contextManager.AddChatHistory(messages);
        contextManager.AddCurrentPrompt(currentMessage);
        return contextManager.GetContext();
    }
    public async Task<ChatEntity> CreateChatAsync(Guid userId, Guid modelId) {
        var chat = new ChatEntity(
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

    public async Task DeleteChatAsync(Guid chatId, Guid userId) {
        var chat = await _dbContext.Chats.Where(x => x.Id == chatId && x.UserId == userId).FirstOrDefaultAsync();
        if (chat == null) {
            throw new NotFoundException("Chat not found");
        }
        
        _dbContext.Chats.Remove(chat);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<ChatWithMessagesDto> GetChatMessagesAsync(Guid chatId, Guid userId) {
        var chat = await _dbContext.Chats
            .AsNoTracking()
            .Where(x => x.Id == chatId && x.UserId == userId)
            .Include(x => x.Model)
            .FirstOrDefaultAsync();
        
        if (chat == null) {
            throw new NotFoundException("Chat not found");
        }
        
        var messages = await _messagesRepository.GetChatMessages(chatId);
        return chat.ToResponse(messages);
    }

    public async Task<List<ChatEntity>> GetAllUserChats(Guid userId) {
        var user  = await _dbContext.Users.AsNoTracking().Where(x => x.Id == userId).FirstOrDefaultAsync();
        if (user == null) {
            throw new NotFoundException("User not found");
        }

        return await _chatsRepository.GetAllUserChats(userId);
    }
    
    public async IAsyncEnumerable<string> ProcessPromptAsync(Guid chatId, Guid userId, string prompt, CancellationToken token) {
        var user = await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == userId)
            .FirstOrDefaultAsync();
        var chat = await _dbContext.Chats
            .AsNoTracking()
            .Where(x => x.Id == chatId && x.UserId == userId)
            .Include(x => x.Model)
            .FirstOrDefaultAsync();
        
        if (user == null || chat == null) {
            throw new NotFoundException();
        }
        if (user.Tokens <= 0) {
            throw new BillingException("Not enough tokens");
        }
        
        await _messageService.SaveMessageAsync(chat, MessageRole.User, prompt);
        var context = await ReadContext(chatId, prompt, chat.Model.Context);
        var stream = await _generationService.StreamGeneration(chat.ModelId, userId, context, token);

        await foreach (var chunk in OllamaClient.DeserializeStream(stream, token)) {
            yield return chunk;
        }
    }
}