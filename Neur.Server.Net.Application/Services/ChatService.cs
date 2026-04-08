using System.Text;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Extensions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Application.Services.Background;
using Neur.Server.Net.Application.Services.DTO.ChatService;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;

namespace Neur.Server.Net.Application.Services;

public class ChatService : IChatService {
    private readonly GenerationService _generationService;
    private readonly IUsersRepository _usersRepository;
    private readonly IModelsRepository _modelsRepository;
    private readonly IChatsRepository _chatsRepository;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IGenerationRequestsRepository _requestsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChatService(GenerationService generationService,
        IChatsRepository chatsRepository, IMessagesRepository messagesRepository,
        IUsersRepository usersRepository, IModelsRepository modelsRepository,
        IGenerationRequestsRepository requestsRepository,
        IUnitOfWork unitOfWork) {
        _usersRepository = usersRepository;
        _modelsRepository = modelsRepository;
        _messagesRepository = messagesRepository;
        _generationService = generationService;
        _requestsRepository = requestsRepository;
        _chatsRepository = chatsRepository;
        _unitOfWork = unitOfWork;
    }
    private async Task<string> ReadContextAsync(Guid chatId, string currentMessage, string baseContext, CancellationToken token = default) {
        List<MessageEntity> messages = await _messagesRepository.GetChatMessagesAsync(chatId, token);
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
        await _unitOfWork.SaveChangesAsync(token);
        return chat;
    }

    public async Task DeleteChatAsync(Guid chatId, CancellationToken token = default) {
        var chat = await _chatsRepository.GetAsync(chatId, false, token);
        if (chat == null) {
            throw new NotFoundException("Chat not found");
        }
        await _chatsRepository.DeleteAsync(chatId, token);
        await _unitOfWork.SaveChangesAsync(token);
    }

    public async Task<ChatWithMessagesDto> GetChatMessagesAsync(Guid chatId, CancellationToken token = default) {
        var chat = await _chatsRepository.GetAsync(chatId, false, token);
        if (chat == null) {
            throw new NotFoundException("Chat not found");
        }
        
        var messages = await _messagesRepository.GetChatMessagesAsync(chatId, token);
        return chat.ToResponse(messages);
    }

    public async Task<List<ChatEntity>> GetAllUserChatsAsync(Guid userId, CancellationToken token = default) {
        var user = await _usersRepository.GetByIdAsync(userId, token: token);
        if (user == null) {
            throw new NotFoundException("User not found");
        }
        return await _chatsRepository.GetAllUserChatsAsync(userId, token);
    }
    
    public async IAsyncEnumerable<string> ProcessPromptAsync(Guid chatId, string prompt, CancellationToken token) {
        var chat = await _chatsRepository.GetAsync(chatId, tracking: true, token: token);
        
        if (chat == null) {
            throw new NotFoundException();
        }

        var user = chat.User;
        if (user.Tokens <= 0) {
            throw new BillingException("Not enough tokens");
        }

        var promptContext = await ReadContextAsync(chatId, prompt, chat.Model.Context, token);
        var messageRequest = new MessageEntity(chatId, MessageRole.User, prompt);
        var generationRequest = new GenerationRequestEntity(user.Id, chat.ModelId, 1, messageRequest.Id);
        
        // Первая операция
        await _messagesRepository.AddAsync(messageRequest, token);
        await _requestsRepository.AddAsync(generationRequest, token);
        await _unitOfWork.SaveChangesAsync(token);
        
        bool completed = false;
        var modelResponse = new StringBuilder();

        // Вторая операция
        var onResponse = async () => {
            generationRequest.MarkInProcess();
            await _unitOfWork.SaveChangesAsync(token);
        };
        
        try {
            await foreach (var chunk in _generationService.StreamGeneration(generationRequest, promptContext, onResponse, token)) {
                modelResponse.Append(chunk);
                yield return chunk;
            }
            completed = true;
        }
        finally {
            if (completed) {
                var messageResponse = new MessageEntity(chatId, MessageRole.Assistant, modelResponse.ToString());
                await _messagesRepository.AddAsync(messageResponse, token);
                generationRequest.MarkSuccessFinished(messageResponse.Id);
                chat.UpdatedAt = DateTime.UtcNow;
                user.Tokens--;
            }
            else {
                generationRequest.MarkFailed();
            }
            await _unitOfWork.SaveChangesAsync(token);
        }
    }
}