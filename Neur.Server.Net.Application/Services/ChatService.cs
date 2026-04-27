using System.Text;
using Microsoft.Extensions.Logging;
using Neur.Server.Net.Application.DTOs.Chat;
using Neur.Server.Net.Application.Exceptions;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Extensions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Application.Services.Background;
using Neur.Server.Net.Application.Services.DTO.ChatService;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

namespace Neur.Server.Net.Application.Services;

public class ChatService : IChatService {
    private readonly GenerationService _generationService;
    private readonly IModelsRepository _modelsRepository;
    private readonly IChatsRepository _chatsRepository;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IGenerationRequestsRepository _requestsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ChatService> _logger;

    public ChatService(GenerationService generationService,
        IChatsRepository chatsRepository, IMessagesRepository messagesRepository, 
        IModelsRepository modelsRepository,
        IGenerationRequestsRepository requestsRepository,
        IUsersRepository usersRepository,
        IUnitOfWork unitOfWork, ILogger<ChatService> logger) {
        _modelsRepository = modelsRepository;
        _messagesRepository = messagesRepository;
        _generationService = generationService;
        _requestsRepository = requestsRepository;
        _chatsRepository = chatsRepository;
        _usersRepository = usersRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    private static string StripThinkTags(string content) {
        var result = string.Empty;
        var lines = content.Split("\n");
        var inThink = false;
        foreach (var line in lines) {
            if (line == "<think>") { inThink = true; continue; }
            if (line == "</think>") { inThink = false; continue; }
            if (!inThink) result += line;
        }
        return result.Trim();
    }

    private OllamaChatRequest BuildChatRequestAsync(List<MessageEntity> messageHistory, string modelName, string systemContext, string currentMessage) {
        var messages = new List<OllamaChatMessage>();

        if (!string.IsNullOrEmpty(systemContext))
            messages.Add(new OllamaChatMessage("system", systemContext));

        foreach (var msg in messageHistory)
            messages.Add(new OllamaChatMessage(msg.Role.ToString().ToLower(), StripThinkTags(msg.Content)));

        messages.Add(new OllamaChatMessage("user", currentMessage));

        _logger.LogInformation("Chat request built: {MessageCount} messages", messages.Count);
        return new OllamaChatRequest(modelName, messages);
    }

    private async Task<(ChatEntity, bool)> GetChatSafelyAsync(Guid userId, Guid? chatId, Guid? modelId, CancellationToken token) {
        ChatEntity? chat;
        bool isNewChat = false;

        // Пустое поле id модели при создании чата - ошибка в запросе
        if (chatId == null && modelId == null) throw new ValidateException("model_id must not be null");
        
        // Создание нового чата
        if (chatId == null) {
            _logger.LogInformation("Chat request returned null chat, creating new chat...");
            chat = await CreateChatAsync(userId, modelId!.Value, token);
            var user = await _usersRepository.GetByIdAsync(userId, true, token); // + in tracking
            isNewChat = true;
            _logger.LogInformation("The new chat was created: {ChatId}", chat.Id);
        }
        else {
            chat = await _chatsRepository.GetAsync(chatId!.Value, tracking: true, token: token);   
        }
        
        if (chat == null) {
            throw new NotFoundException("Chat not found");
        }

        if (chat.Model == null || chat.ModelId == null) {
            throw new ModelDeletedException();
        }

        return (chat, isNewChat);
    }
    
    public async Task<ChatEntity> CreateChatAsync(Guid userId, Guid modelId, CancellationToken token = default) {
        var model = await _modelsRepository.GetAsync(modelId,true, token);
        if (model == null) {
            throw new NotFoundException("Model not found");
        }
        var chat = new ChatEntity(
            userId: userId,
            modelId: modelId,
            createdAt:  DateTime.UtcNow
        );
        await _chatsRepository.AddAsync(chat, token);
        await _unitOfWork.SaveChangesAsync(token);
        _logger.LogInformation("Chat {ChatId} created with model {ModelId}", chat.Id, modelId);
        return chat;
    }

    public async Task DeleteChatAsync(Guid chatId, CancellationToken token = default) {
        var chat = await _chatsRepository.GetAsync(chatId, false, token);
        if (chat == null) {
            throw new NotFoundException("Chat not found");
        }
        await _chatsRepository.DeleteAsync(chatId, token);
        await _unitOfWork.SaveChangesAsync(token);
        _logger.LogInformation("Chat {ChatId} deleted", chatId);
    }

    public async Task<ChatWithMessagesDto> GetChatWithMessagesAsync(Guid userId, Guid chatId, CancellationToken token = default) {
        var chat = await _chatsRepository.GetAsync(chatId, false, token);
        if (chat == null) {
            throw new NotFoundException("Chat not found");
        }
        var messages = await _messagesRepository.GetUserMessagesAsync(userId, chatId, token);
        // Чат не принадлежит пользователю
        if (!messages.Any()) {
            throw new NotFoundException("Chat not found");
        }
        return chat.ToResponse(messages);
    }

    public async Task<List<ChatEntity>> GetAllUserChatsAsync(Guid userId, CancellationToken token = default) {
        return await _chatsRepository.GetAllUserChatsAsync(userId, token);
    }
    
    public async IAsyncEnumerable<GenerationChunkResponse> ProcessPromptAsync(Guid userId, Guid? chatId, Guid? modelId, string prompt, CancellationToken token) {
        _logger.LogInformation("Processing prompt in chat {ChatId}", chatId);
        
        var (chat, isNewChat) = await GetChatSafelyAsync(userId, chatId, modelId, token);
        var user = chat.User;
        
        if (user.Tokens <= 0) {
            _logger.LogInformation("User {UserId} has no tokens left", user.Id);
            throw new BillingException("Not enough tokens");
        }

        var chatMessages = await _messagesRepository.GetUserMessagesAsync(user.Id, chat.Id, token);
        var chatRequest = BuildChatRequestAsync(chatMessages, chat.Model!.ModelName, chat.Model.Context, prompt);
        var messageRequest = new MessageEntity(chat.Id, MessageRole.User, prompt);
        var generationRequest = new GenerationRequestEntity(user.Id, chat.ModelId!.Value, 1, messageRequest.Id, 
            chat.Model.Name, chat.Model.ModelName);

        // Первая операция
        await _messagesRepository.AddAsync(messageRequest, token);
        await _requestsRepository.AddAsync(generationRequest, token);
        await _unitOfWork.SaveChangesAsync(token);

        _logger.LogInformation("Generation {RequestId} queued, model {ModelId}",
            generationRequest.Id, chat.ModelId);

        bool completed = false;
        var modelResponse = new StringBuilder();

        // Вторая операция
        var onResponse = async () => {
            generationRequest.MarkInProcess();
            await _unitOfWork.SaveChangesAsync(token);
            _logger.LogInformation("Generation {RequestId} started", generationRequest.Id);
        };

        try {
            if (isNewChat) yield return new GenerationChunkResponse(GenerationChunkResponseType.Meta, chat.Id.ToString(), false);
            await foreach (var chunk in _generationService.StreamGeneration(generationRequest, chatRequest, onResponse, token)) {
                modelResponse.Append(chunk);
                yield return new GenerationChunkResponse(GenerationChunkResponseType.Data, chunk, false);
            }
            yield return new GenerationChunkResponse(GenerationChunkResponseType.Data, "", true);
            completed = true;
        }
        finally {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            if (completed) {
                var messageResponse = new MessageEntity(chat.Id, MessageRole.Assistant, modelResponse.ToString());
                await _messagesRepository.AddAsync(messageResponse, cts.Token);
                generationRequest.MarkSuccessFinished(messageResponse.Id);
                chat.UpdatedAt = DateTime.UtcNow;
                user.Tokens--;
                _logger.LogInformation("Generation {RequestId} completed, tokens left: {Tokens}",
                    generationRequest.Id, user.Tokens);
            }
            else {
                generationRequest.MarkFailed();
                _logger.LogInformation("Generation {RequestId} failed or cancelled", generationRequest.Id);
            }
            await _unitOfWork.SaveChangesAsync(cts.Token);
        }
    }
}