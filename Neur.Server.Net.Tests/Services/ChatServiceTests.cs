using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Neur.Server.Net.Application.DTOs.Chat;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure.Clients.Contracts.OllamaClient;

public class ChatServiceTests {
    private readonly Mock<IGenerationService> _generationService = new();
    private readonly Mock<IChatsRepository> _chatsRepository = new();
    private readonly Mock<IMessagesRepository> _messagesRepository = new();
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<IModelsRepository> _modelsRepository = new();
    private readonly Mock<IGenerationRequestsRepository> _requestsRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly ChatService _sut;

    public ChatServiceTests() {
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _messagesRepository.Setup(x => x.AddAsync(It.IsAny<MessageEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _requestsRepository.Setup(x => x.AddAsync(It.IsAny<GenerationRequestEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _sut = new ChatService(
            _generationService.Object,
            _chatsRepository.Object,
            _messagesRepository.Object,
            _modelsRepository.Object,
            _requestsRepository.Object,
            _usersRepository.Object,
            _unitOfWork.Object,
            Mock.Of<ILogger<ChatService>>()
        );
    }

    private static UserEntity MakeUser(Guid id, int tokens = 10) =>
        new UserEntity("testuser", "Test", "User", UserRole.Student, tokens) { Id = id };

    private static ModelEntity MakeModel(Guid? id = null) =>
        new ModelEntity(
            id ?? Guid.NewGuid(),
            "test", "test-model", "context", ModelType.Text, "1.0", ModelStatus.open, DateTime.UtcNow
        );

    private static ChatEntity MakeChat(Guid chatId, Guid userId, UserEntity user, ModelEntity? model = null) {
        var m = model ?? MakeModel();
        return new ChatEntity(m.Id, userId, DateTime.UtcNow) {
            Id = chatId,
            User = user,
            Model = m
        };
    }

    [Fact]
    public async Task ProcessPromptAsync_ChatNotFound_ThrowsNotFoundException() {
        _chatsRepository
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatEntity?)null);

        var act = () => _sut.ProcessPromptAsync(Guid.NewGuid(), Guid.NewGuid(), null, "hello", CancellationToken.None)
            .ToListAsync().AsTask();

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ProcessPromptAsync_NotEnoughTokens_ThrowsBillingException() {
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var user = MakeUser(userId, tokens: 0);

        _chatsRepository
            .Setup(x => x.GetAsync(chatId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeChat(chatId, userId, user));

        var act = () => _sut.ProcessPromptAsync(userId, chatId, null, "hello", CancellationToken.None)
            .ToListAsync().AsTask();

        await act.Should().ThrowAsync<BillingException>()
            .WithMessage("*tokens*");
    }

    [Fact]
    public async Task ProcessPromptAsync_ValidRequest_ReturnsChunksFromGenerationService() {
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var user = MakeUser(userId);
        var chunks = new[] { "Hello", " world" };

        _chatsRepository
            .Setup(x => x.GetAsync(chatId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeChat(chatId, userId, user));
        _messagesRepository
            .Setup(x => x.GetUserMessagesAsync(userId, chatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MessageEntity>());
        _generationService
            .Setup(x => x.StreamGeneration(
                It.IsAny<GenerationRequestEntity>(),
                It.IsAny<OllamaChatRequest>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns(chunks.ToAsyncEnumerable());

        var result = await _sut.ProcessPromptAsync(userId, chatId, null, "hello", CancellationToken.None)
            .ToListAsync();

        result.Where(r => r.Type == GenerationChunkResponseType.Data && !r.IsCompleted)
            .Select(r => r.Data)
            .Should().Equal(chunks);
    }

    [Fact]
    public async Task ProcessPromptAsync_ValidRequest_SavesUserAndAssistantMessages() {
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var user = MakeUser(userId);

        _chatsRepository
            .Setup(x => x.GetAsync(chatId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeChat(chatId, userId, user));
        _messagesRepository
            .Setup(x => x.GetUserMessagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MessageEntity>());
        _generationService
            .Setup(x => x.StreamGeneration(
                It.IsAny<GenerationRequestEntity>(),
                It.IsAny<OllamaChatRequest>(),
                It.IsAny<Func<Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns(new[] { "response" }.ToAsyncEnumerable());

        await _sut.ProcessPromptAsync(userId, chatId, null, "user prompt", CancellationToken.None)
            .ToListAsync();

        _messagesRepository.Verify(
            x => x.AddAsync(It.Is<MessageEntity>(m => m.Role == MessageRole.User && m.Content == "user prompt"), It.IsAny<CancellationToken>()),
            Times.Once
        );
        _messagesRepository.Verify(
            x => x.AddAsync(It.Is<MessageEntity>(m => m.Role == MessageRole.Assistant && m.Content == "response"), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateChatAsync_ModelNotFound_ThrowsNotFoundException() {
        _modelsRepository
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModelEntity?)null);

        var act = () => _sut.CreateChatAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateChatAsync_ValidRequest_CreateUserChat() {
        var userId = Guid.NewGuid();
        var modelId = Guid.NewGuid();

        _modelsRepository
            .Setup(x => x.GetAsync(modelId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeModel(modelId));
        _chatsRepository
            .Setup(x => x.AddAsync(It.IsAny<ChatEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var result = await _sut.CreateChatAsync(userId, modelId, CancellationToken.None);

        result.UserId.Should().Be(userId);
        result.ModelId.Should().Be(modelId);
    }
}
