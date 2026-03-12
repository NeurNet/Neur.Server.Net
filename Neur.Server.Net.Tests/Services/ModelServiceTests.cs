using Microsoft.Extensions.Configuration;
using MockQueryable.Moq;
using Moq;
using FluentAssertions;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Postgres;

public class ModelServiceTests {
    private readonly Mock<ApplicationDbContext> _context = new(Mock.Of<IConfiguration>());
    private readonly Mock<IModelsRepository> _modelsRepository = new();
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly ModelService _sut;

    public ModelServiceTests() {
        _sut = new ModelService(_context.Object, _modelsRepository.Object, _usersRepository.Object);
    }

    private static ModelEntity MakeModel(Guid? id = null, ModelStatus status = ModelStatus.open) =>
        new ModelEntity(
            id ?? Guid.NewGuid(),
            "Test Model", "test-model", "some context",
            ModelType.text, "1.0", status,
            DateTime.UtcNow
        );

    private static UserEntity MakeUser(Guid id, UserRole role = UserRole.Student) =>
        new UserEntity("user", "Name", "Surname", role, 10) { Id = id };

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_SavesAndReturnsModel() {
        var model = MakeModel();

        _modelsRepository
            .Setup(x => x.AddAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _modelsRepository
            .Setup(x => x.GetAsync(model.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        var result = await _sut.CreateAsync(model);

        result.Should().Be(model);
    }

    [Fact]
    public async Task CreateAsync_WhenGetAfterSaveReturnsNull_ThrowsException() {
        var model = MakeModel();

        _modelsRepository
            .Setup(x => x.AddAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _modelsRepository
            .Setup(x => x.GetAsync(model.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModelEntity?)null);

        var act = () => _sut.CreateAsync(model);

        await act.Should().ThrowAsync<Exception>().WithMessage("*create*");
    }

    // --- GetAsync ---

    [Fact]
    public async Task GetAsync_WhenModelExists_ReturnsModel() {
        var model = MakeModel();

        _modelsRepository
            .Setup(x => x.GetAsync(model.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);

        var result = await _sut.GetAsync(model.Id);

        result.Should().Be(model);
    }

    [Fact]
    public async Task GetAsync_WhenModelNotFound_ThrowsNotFoundException() {
        _modelsRepository
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModelEntity?)null);

        var act = () => _sut.GetAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Model not found*");
    }

    // --- GetAllByUserRoleAsync ---

    [Fact]
    public async Task GetAllByUserRoleAsync_UserNotFound_ThrowsNotFoundException() {
        _usersRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var act = () => _sut.GetAllByUserRoleAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*User not found*");
    }

    [Fact]
    public async Task GetAllByUserRoleAsync_AdminUser_ReturnsAllModels() {
        var userId = Guid.NewGuid();
        var models = new List<ModelEntity> {
            MakeModel(status: ModelStatus.open),
            MakeModel(status: ModelStatus.locked)
        };

        _usersRepository
            .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(userId, UserRole.Admin));
        _context.Setup(x => x.Models).Returns(models.BuildMockDbSet().Object);

        var result = await _sut.GetAllByUserRoleAsync(userId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllByUserRoleAsync_StudentUser_ReturnsOnlyOpenModels() {
        var userId = Guid.NewGuid();
        var models = new List<ModelEntity> {
            MakeModel(status: ModelStatus.open),
            MakeModel(status: ModelStatus.locked)
        };

        _usersRepository
            .Setup(x => x.GetByIdAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeUser(userId, UserRole.Student));
        _context.Setup(x => x.Models).Returns(models.BuildMockDbSet().Object);

        var result = await _sut.GetAllByUserRoleAsync(userId);

        result.Should().HaveCount(1);
        result.Single().Status.Should().Be(ModelStatus.open);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ModelNotFound_ThrowsNotFoundException() {
        var model = MakeModel();

        _modelsRepository
            .Setup(x => x.GetAsync(model.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModelEntity?)null);

        var act = () => _sut.UpdateAsync(model);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Model not found*");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAllFieldsAndCallsSaveChanges() {
        var id = Guid.NewGuid();
        var existing = MakeModel(id);
        var updated = new ModelEntity(id, "New Name", "new-model", "new context", ModelType.code, "2.0", ModelStatus.locked, DateTime.UtcNow);

        _modelsRepository
            .Setup(x => x.GetAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _context
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _sut.UpdateAsync(updated);

        existing.Name.Should().Be("New Name");
        existing.ModelName.Should().Be("new-model");
        existing.Context.Should().Be("new context");
        existing.Type.Should().Be(ModelType.code);
        existing.Version.Should().Be("2.0");
        existing.Status.Should().Be(ModelStatus.locked);
        existing.UpdatedAt.Should().NotBeNull();
        _context.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_ModelNotFound_ThrowsNotFoundException() {
        _modelsRepository
            .Setup(x => x.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ModelEntity?)null);

        var act = () => _sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>().WithMessage("*Model not found*");
    }

    [Fact]
    public async Task DeleteAsync_WhenModelExists_CallsRepositoryDelete() {
        var model = MakeModel();

        _modelsRepository
            .Setup(x => x.GetAsync(model.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(model);
        _modelsRepository
            .Setup(x => x.DeleteAsync(model, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeleteAsync(model.Id);

        _modelsRepository.Verify(x => x.DeleteAsync(model, It.IsAny<CancellationToken>()), Times.Once);
    }
}
