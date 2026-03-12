using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;

public class ChatServiceTests {
    private readonly Mock<IGenerationService> _generationService = new();
    private readonly Mock<IMessageService> _messageService = new();
    private readonly Mock<IChatsRepository> _chatsRepository = new();
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<IModelsRepository> _modelsRepository = new();

    private readonly ChatService _sut;

    public ChatServiceTests() {
        _sut = new ChatService(
            _generationService.Object,
            _chatsRepository.Object,
            _messageService.Object,
            _usersRepository.Object,
            _modelsRepository.Object
        );
    }

    // --- Хелперы для создания тестовых данных ---

    private static UserEntity MakeUser(Guid id, int tokens = 10) =>
        new UserEntity("testuser", "Test", "User", UserRole.Student, tokens) { Id = id };

    private static ChatEntity MakeChat(Guid id, Guid userId, Guid? modelId = null) =>
        new ChatEntity(Guid.NewGuid(), userId, DateTime.UtcNow) {
            Id = id,
            Model = MakeModel(modelId ?? Guid.NewGuid())
        };

    private static ModelEntity MakeModel(Guid id) =>
        new ModelEntity("test", "test-model", "4096", ModelType.text, "1.0", ModelStatus.open, DateTime.UtcNow) {
            Id = id,
        };

    // --- Тесты ---

    [Fact]
    public async Task ProcessPromptAsync_UserNotFound_ThrowsNotFoundException() {
        _usersRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);
        _chatsRepository
            .Setup(x => x.GetWithModelAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChatEntity?)null);

        var act = () => _sut.ProcessPromptAsync(Guid.NewGuid(), Guid.NewGuid(), "hello", CancellationToken.None)
            .ToListAsync().AsTask();

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ProcessPromptAsync_NotEnoughTokens_ThrowsBillingException() {
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();

        _usersRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(userId, tokens: 0));
        _chatsRepository
            .Setup(x => x.GetWithModelAsync(chatId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeChat(chatId, userId));

        var act = () => _sut.ProcessPromptAsync(chatId, userId, "hello", CancellationToken.None)
            .ToListAsync().AsTask();

        await act.Should().ThrowAsync<BillingException>()
            .WithMessage("*tokens*");
    }

    [Fact]
    public async Task ProcessPromptAsync_ValidRequest_ReturnsChunksFromGenerationService() {
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();
        var chunks = new[] { "Hello", " world" };

        _usersRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(userId));
        _chatsRepository
            .Setup(x => x.GetWithModelAsync(chatId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeChat(chatId, userId));

        _messageService
            .Setup(x => x.GetChatMessagesAsync(chatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MessageEntity>());
        _generationService
            .Setup(x => x.StreamGeneration(It.IsAny<Guid>(), userId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(chunks.ToAsyncEnumerable());
        _messageService
            .Setup(x => x.SaveMessageAsync(It.IsAny<ChatEntity>(), It.IsAny<MessageRole>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var result = await _sut.ProcessPromptAsync(chatId, userId, "hello", CancellationToken.None)
            .ToListAsync();

        result.Should().Equal(chunks);
    }

    [Fact]
    public async Task ProcessPromptAsync_ValidRequest_SavesUserAndAssistantMessages() {
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();

        _usersRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(userId));
        _chatsRepository
            .Setup(x => x.GetWithModelAsync(chatId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeChat(chatId, userId));

        _messageService
            .Setup(x => x.GetChatMessagesAsync(chatId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MessageEntity>());
        _generationService
            .Setup(x => x.StreamGeneration(It.IsAny<Guid>(), userId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(new[] { "response" }.ToAsyncEnumerable());
        _messageService
            .Setup(x => x.SaveMessageAsync(It.IsAny<ChatEntity>(), It.IsAny<MessageRole>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        await _sut.ProcessPromptAsync(chatId, userId, "user prompt", CancellationToken.None)
            .ToListAsync();

        _messageService.Verify(
            x => x.SaveMessageAsync(It.IsAny<ChatEntity>(), MessageRole.User, "user prompt", It.IsAny<CancellationToken>()),
            Times.Once
        );
        _messageService.Verify(
            x => x.SaveMessageAsync(It.IsAny<ChatEntity>(), MessageRole.Assistant, "response", It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateChatAsync_ModelNotFound_ThrowsNotFoundException() {
        _modelsRepository
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModelEntity?)null);

        var act = () => _sut.CreateChatAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateChatAsync_ValidRequest_CreateUserChat() {
        var userId = Guid.NewGuid();
        var modelId = Guid.NewGuid();

        _modelsRepository
            .Setup(x => x.GetAsync(modelId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeModel(modelId));
        _chatsRepository
            .Setup(x => x.AddAsync(It.IsAny<ChatEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var result = await _sut.CreateChatAsync(userId, modelId, CancellationToken.None);

        result.UserId.Should().Be(userId);
        result.ModelId.Should().Be(modelId);
    }
}