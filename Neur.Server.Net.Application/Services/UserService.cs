using Microsoft.Extensions.Logging;
using Neur.Server.Net.Application.Exceptions;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Clients;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure.Clients.Contracts.CollegeClient;
using Neur.Server.Net.Infrastructure.Interfaces;

namespace Neur.Server.Net.Application.Services;

public class UserService : IUserService {
    private readonly IUsersRepository _usersRepository;
    private readonly IGenerationRequestsRepository _requestsRepository;
    private readonly ICollegeClient _collegeClient;
    private readonly IJwtProvider _jwtProvider;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;

    public UserService(IUsersRepository usersRepository, IGenerationRequestsRepository requestsRepository, ICollegeClient collegeClient, IJwtProvider jwtProvider, IUnitOfWork unitOfWork, ILogger<UserService> logger) {
        _usersRepository = usersRepository;
        _requestsRepository = requestsRepository;
        _collegeClient = collegeClient;
        _jwtProvider = jwtProvider;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    private UserRole DeterminateRole(string role) {
        switch (role) {
            case "student":
                return UserRole.Student;
            case "teacher":
                return UserRole.Teacher;
            case "admin":
                return UserRole.Admin;
        }
        throw new Exception("UserRole doesn't exist");
    }
    public async Task<string> Login(string username, string password, CancellationToken cancellationToken = default) {
        var collegeUser = await _collegeClient.AuthenticateAsync(username, password, cancellationToken);
        if (collegeUser == null) {
            throw new NotAuthorizedException();
        }

        try {
            var user = await _usersRepository.GetByLdapIdAsync(username);
            var token = _jwtProvider.GenerateToken(user);
            _logger.LogInformation("User logged in");
            return token;
        }
        catch (NullReferenceException ex) {
            var name = collegeUser.username.Split()[0];
            var surname = collegeUser.username.Split()[1];

            var newUser = new UserEntity(
                username: collegeUser.id,
                name: name,
                surname: surname,
                role: DeterminateRole(collegeUser.role),
                tokens: 10
            );
            await _usersRepository.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();
            var token = _jwtProvider.GenerateToken(newUser);
            _logger.LogInformation("New user registered with role {Role}", newUser.Role);
            return token;
        }
    }

    public async Task<List<(UserEntity, DateTime?)>> GetAllUsersWithLastRequest(CancellationToken token = default) {
        return await _usersRepository.GetAllWithLastRequestTimeAsync(token);
    }

    public async Task ChangeUserRole(Guid userId, UserRole role) {
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null) {
            throw new NotFoundException("User not found");
        }

        await _usersRepository.UpdateRoleAsync(userId, role);
        _logger.LogInformation("Role changed to {Role} for user {TargetUserId}", role, userId);
    }
}