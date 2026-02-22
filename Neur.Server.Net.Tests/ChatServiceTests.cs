using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using MockQueryable.Moq;
using FluentAssertions;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Postgres;

public class ChatServiceTests {
    // Моки зависимостей
    private readonly Mock<ApplicationDbContext> _dbContext = new(Mock.Of<IConfiguration>());
    private readonly Mock<IGenerationService> _generationService = new();
    private readonly Mock<IMessageService> _messageService = new();
    private readonly Mock<IChatsRepository> _chatsRepository = new();
    private readonly Mock<IMessagesRepository> _messagesRepository = new();

    private readonly ChatService _sut;

    public ChatServiceTests() {
        _sut = new ChatService(
            _dbContext.Object,
            _generationService.Object,
            _chatsRepository.Object,
            _messagesRepository.Object,
            _messageService.Object
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

    private void SetupDbUsers(params UserEntity[] users) {
        var mock = users.BuildMockDbSet();
        _dbContext.Setup(x => x.Users).Returns(mock.Object);
    }

    private void SetupDbChats(params ChatEntity[] chats) {
        var mock = chats.BuildMockDbSet();
        _dbContext.Setup(x => x.Chats).Returns(mock.Object);
    }

    private void SetupDbModels(params ModelEntity[] models) {
        var mock = models.BuildMockDbSet();
        _dbContext.Setup(x => x.Models).Returns(mock.Object);
    }

    // --- Тесты ---

    [Fact]
    public async Task ProcessPromptAsync_UserNotFound_ThrowsNotFoundException() {
        SetupDbUsers(); // пустая таблица
        SetupDbChats();

        var act = () => _sut.ProcessPromptAsync(Guid.NewGuid(), Guid.NewGuid(), "hello", CancellationToken.None)
            .ToListAsync().AsTask();

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ProcessPromptAsync_NotEnoughTokens_ThrowsBillingException() {
        var userId = Guid.NewGuid();
        var chatId = Guid.NewGuid();

        SetupDbUsers(MakeUser(userId, tokens: 0)); // баланс нулевой
        SetupDbChats(MakeChat(chatId, userId));

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

        SetupDbUsers(MakeUser(userId));
        SetupDbChats(MakeChat(chatId, userId));

        _messagesRepository
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

        SetupDbUsers(MakeUser(userId));
        SetupDbChats(MakeChat(chatId, userId));

        _messagesRepository
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

        // Проверяем что сохранились оба сообщения: от юзера и от ассистента
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
        var userId = Guid.NewGuid();
        
        SetupDbUsers(MakeUser(userId));
        SetupDbModels();
        var chat = MakeChat(Guid.NewGuid(), userId);

        var act = () => _sut.CreateChatAsync(chat.Id, Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateChatAsync_ValidRequest_CreateUserChat() {
        var userId = Guid.NewGuid();
        var modelId = Guid.NewGuid();
        
        SetupDbModels(MakeModel(modelId));

        _chatsRepository
            .Setup(x => x.AddAsync(It.IsAny<ChatEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task<Guid>.Factory.StartNew(() => Guid.NewGuid()));
        
        var result = await _sut.CreateChatAsync(userId, modelId, CancellationToken.None);
        
        result.UserId.Should().Be(userId);
        result.ModelId.Should().Be(modelId);
    }
}