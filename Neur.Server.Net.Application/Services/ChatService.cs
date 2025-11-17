using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Application.Services;

public class ChatService {
    private readonly BillingService _billingService;
    private readonly IRequestsRepository _requestsRepository;
    private readonly IUsersRepository _usersRepository;
    private readonly LLMService _llmService;
    public ChatService(BillingService billingService, IRequestsRepository requestsRepository, IUsersRepository usersRepository, LLMService llmService) {
        _billingService = billingService;
        _requestsRepository = requestsRepository;
        _usersRepository = usersRepository;
        _llmService = llmService;
    }
    
    public async IAsyncEnumerable<string> ProcessMessage(Guid userId, Guid chatId, string message) {
        var user = await _usersRepository.GetById(userId);
        var request = RequestEntity.Create(
            id: Guid.NewGuid(),
            chatId: chatId,
            message,
            createdAt: DateTime.UtcNow
        );
        await _requestsRepository.Add(request);
        var savedRequest = await _requestsRepository.Get(request.Id);
        
        var session = _llmService.GetSession(savedRequest);
        await _billingService.ConsumeTokensAsync(user.Id, 1);
        await foreach (var chunk in session.StreamResponse()) {
            yield return chunk;
        }
    }
}