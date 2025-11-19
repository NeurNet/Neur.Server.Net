using Neur.Server.Net.Application.Clients;
using Neur.Server.Net.Application.Clients.Contracts.OllamaClient;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;

namespace Neur.Server.Net.Application.Services;

public class LLMService {
    private readonly OllamaClient _client;
    private readonly IMessagesRepository _messagesRepository;
    private readonly IModelsRepository _modelsRepository;
    public LLMService(OllamaClient client, IMessagesRepository repository, IModelsRepository modelsRepository) {
        _client = client;
        _messagesRepository = repository;
        _modelsRepository = modelsRepository;
    }
    
    private async Task<string> ReadContext(Guid chatId, string currentMessage, string baseContext) {
        List<MessageEntity> messages = await _messagesRepository.GetChatMessages(chatId);
        var contextManager = new ContextManager();
        
        contextManager.AddBaseContext(baseContext);
        contextManager.AddChatHistory(messages);
        contextManager.AddCurrentPrompt(currentMessage);
        return contextManager.GetContext();
    }

    public async IAsyncEnumerable<string> StreamOllamaResponse(ChatEntity chat, string promt) {
        var model = await _modelsRepository.Get(chat.ModelId);
        var context = await ReadContext(chat.Id, promt, model.Context);
        var ollamaRequest = new OllamaRequest(chat.Model.ModelName, context, true);
        Console.WriteLine(ollamaRequest.prompt);
        var userMessage = MessageEntity.Create(
            chat.Id,
            DateTime.UtcNow,
            MessageRole.User,
            promt
        );
        await _messagesRepository.Add(userMessage);
        
        var requestResponse = "";
        
        await foreach (var response in _client.StreamResponse(ollamaRequest)) {
            requestResponse += response;
            yield return response;
        }
        
        var llmMessage = MessageEntity.Create(
            chat.Id,
            DateTime.UtcNow,
            MessageRole.Assistant,
            requestResponse
        );
        await _messagesRepository.Add(llmMessage);
    }
}