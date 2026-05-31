using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Neur.Server.Net.Application.Exceptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Application.Services;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;
using Neur.Server.Net.Infrastructure.Clients.Contracts.CollegeClient;
using Neur.Server.Net.Infrastructure.Interfaces;

namespace Neur.Server.Net.Tests;

public class UserServiceTests {
    private readonly Mock<IUsersRepository> _usersRepository = new();
    private readonly Mock<ICollegeClient> _collegeClient = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly IJwtProvider _jwtProvider;
    private readonly IUserService _userService;

    public UserServiceTests() {
        var options = new JwtOptions {
            SecretKey = "test-key-at-least-32-characters-long!",
            ExpiresHours = 12
        };
        _jwtProvider = new JwtProvider(Options.Create(options));
        _unitOfWork.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userService = new UserService(
            _usersRepository.Object,
            _collegeClient.Object,
            _jwtProvider,
            _unitOfWork.Object,
            Mock.Of<ILogger<UserService>>()
        );
    }

    private UserEntity MakeUser(string username = "i24s0202", string name = "Петя", string surname = "Петросян", UserRole role = UserRole.Student, int tokens = 10) {
        return new UserEntity(username, name, surname, role, tokens);
    }

    [Fact]
    public async Task NewCollegeUserLogin_SavesAndReturnsToken() {
        var userId = "i24s0202";
        var password = "123";

        _collegeClient
            .Setup(x => x.AuthenticateAsync(userId, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthUserResponse(userId, "student", "Петя Петросян"));

        _usersRepository
            .Setup(x => x.GetByLdapIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NullReferenceException());

        _usersRepository
            .Setup(x => x.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _userService.Login(userId, password);
        result.Should().BeOfType<string>();
    }

    [Fact]
    public async Task Login_CollegeReturnsNull_ThrowsNotAuthorizedException() {
        var userId = "i24s0202";
        var password = "wrong";

        _collegeClient
            .Setup(x => x.AuthenticateAsync(userId, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthUserResponse?)null);

        var act = () => _userService.Login(userId, password);

        await act.Should().ThrowAsync<NotAuthorizedException>();
    }

    [Fact]
    public async Task Login_ExistingUser_ReturnsToken() {
        var userId = "i24s0202";
        var password = "123";
        var existingUser = MakeUser(userId);

        _collegeClient
            .Setup(x => x.AuthenticateAsync(userId, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthUserResponse(userId, "student", "Петя Петросян"));

        _usersRepository
            .Setup(x => x.GetByLdapIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var result = await _userService.Login(userId, password);

        result.Should().BeOfType<string>().And.NotBeNullOrEmpty();
        _usersRepository.Verify(x => x.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task NewCollegeUserLogin_AddsUserToRepository() {
        var userId = "i24s0202";
        var password = "123";

        _collegeClient
            .Setup(x => x.AuthenticateAsync(userId, password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthUserResponse(userId, "student", "Петя Петросян"));

        _usersRepository
            .Setup(x => x.GetByLdapIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NullReferenceException());

        _usersRepository
            .Setup(x => x.AddAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _userService.Login(userId, password);

        _usersRepository.Verify(x => x.AddAsync(
            It.Is<UserEntity>(u => u.Username == userId && u.Role == UserRole.Student && u.Tokens == 10),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersWithLastRequest_ReturnsUserList() {
        var users = new List<UserEntity> { MakeUser(), MakeUser("i24s0203", "Вася", "Иванов") };
        var withTime = users.Select(u => (u, (DateTime?)null)).ToList();

        _usersRepository
            .Setup(x => x.GetAllWithLastRequestTimeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(withTime);

        var result = await _userService.GetAllUsersWithLastRequest();

        result.Select(x => x.Item1).Should().BeEquivalentTo(users);
    }

    [Fact]
    public async Task ChangeUserRole_UserNotFound_ThrowsNotFoundException() {
        var actorId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _usersRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var act = () => _userService.ChangeUserRole(actorId, userId, UserRole.Teacher);

        await act.Should().ThrowAsync<Neur.Server.Net.Application.Exeptions.NotFoundException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task ChangeUserRole_UserExists_CallsUpdateRole() {
        var actorId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = MakeUser();

        _usersRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _usersRepository
            .Setup(x => x.UpdateRoleAsync(userId, UserRole.Teacher, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _userService.ChangeUserRole(actorId, userId, UserRole.Teacher);

        _usersRepository.Verify(x => x.UpdateRoleAsync(userId, UserRole.Teacher, It.IsAny<CancellationToken>()), Times.Once);
    }
}
