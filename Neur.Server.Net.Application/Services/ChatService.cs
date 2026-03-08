using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Extensions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Application.Services.DTO.ChatService;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;

namespace Neur.Server.Net.Application.Services;

public class ChatService : IChatService {
    private readonly IUsersRepository _usersRepository;
    private readonly IModelsRepository _modelsRepository;
    private readonly IGenerationService _generationService;
    private readonly IMessageService _messageService;
    private readonly IChatsRepository _chatsRepository;

    public ChatService(IGenerationService generationService,
        IChatsRepository chatsRepository, IMessageService messageService,
        IUsersRepository usersRepository, IModelsRepository modelsRepository) {
        _usersRepository = usersRepository;
        _modelsRepository = modelsRepository;
        _generationService = generationService;
        _chatsRepository = chatsRepository;
        _messageService = messageService;
    }
    private async Task<string> ReadContextAsync(Guid chatId, string currentMessage, string baseContext, CancellationToken token = default) {
        List<MessageEntity> messages = await _messageService.GetChatMessagesAsync(chatId, token);
        var contextManager = new ContextManager(1000);
        
        contextManager.AddBaseContext(baseContext);
        contextManager.AddChatHistory(messages);
        contextManager.AddCurrentPrompt(currentMessage);
        return contextManager.GetContext();
    }
    public async Task<ChatEntity> CreateChatAsync(Guid userId, Guid modelId, CancellationToken token = default) {
        var model = await _modelsRepository.GetAsync(modelId, token);
        if (model == null) {
            throw new NotFoundException("Model not found");
        }
        var chat = new ChatEntity(
            userId: userId,
            modelId: modelId,
            createdAt:  DateTime.UtcNow
        );
        await _chatsRepository.AddAsync(chat, token);
        return chat;
    }

    public async Task DeleteChatAsync(Guid chatId, Guid userId, CancellationToken token = default) {
        var chat = await _chatsRepository.GetAsync(chatId, false, token);
        if (chat == null) {
            throw new NotFoundException("Chat not found");
        }
        await _chatsRepository.DeleteAsync(chatId, token);
    }

    public async Task<ChatWithMessagesDto> GetChatMessagesAsync(Guid chatId, Guid userId, CancellationToken token = default) {
        var chat = await _chatsRepository.GetAsync(chatId, false, token);
        if (chat == null) {
            throw new NotFoundException("Chat not found");
        }
        
        var messages = await _messageService.GetChatMessagesAsync(chatId, token);
        return chat.ToResponse(messages);
    }

    public async Task<List<ChatEntity>> GetAllUserChatsAsync(Guid userId, CancellationToken token = default) {
        var user = await _usersRepository.GetByIdAsync(userId, token: token);
        if (user == null) {
            throw new NotFoundException("User not found");
        }
        return await _chatsRepository.GetAllUserChatsAsync(userId, token);
    }
    
    public async IAsyncEnumerable<string> ProcessPromptAsync(Guid chatId, Guid userId, string prompt, CancellationToken token) {
        var user = await _usersRepository.GetByIdAsync(userId, true, token: token);
        var chat = await _chatsRepository.GetWithModelAsync(chatId, tracking: true, token: token);
        
        if (user == null || chat == null) {
            throw new NotFoundException();
        }
        if (user.Tokens <= 0) {
            throw new BillingException("Not enough tokens");
        }
        
        await _messageService.SaveMessageAsync(chat, MessageRole.User, prompt, token);
        var context = await ReadContextAsync(chatId, prompt, chat.Model.Context, token);
        var modelResponse = string.Empty;
        await foreach (var chunk in _generationService.StreamGeneration(chat.ModelId, userId, context, token)) {
            modelResponse += chunk;
            yield return chunk;
        }
        chat.UpdatedAt = DateTime.UtcNow;
        await _messageService.SaveMessageAsync(chat, MessageRole.Assistant, modelResponse, token);
    }
}