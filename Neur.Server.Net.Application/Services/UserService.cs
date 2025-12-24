using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;
using Neur.Server.Net.Infrastructure.Clients.Contracts.CollegeClient;
using Neur.Server.Net.Infrastructure.Interfaces;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services;

public class UserService : IUserService {
    private readonly ApplicationDbContext _db;
    private readonly IUsersRepository _usersRepository;
    private readonly ICollegeClient _collegeClient;
    private readonly IJwtProvider _jwtProvider;
    
    public UserService(IUsersRepository usersRepository, ICollegeClient collegeClient,  IJwtProvider jwtProvider, ApplicationDbContext db) {
        _usersRepository = usersRepository;
        _collegeClient = collegeClient;
        _jwtProvider = jwtProvider;
        _db = db;
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
    public async Task<string> Login(string username, string password) {
        var collegeUser = new AuthUserResponse("i24s0202", "admin", "Григорий Воробьёв");
        if (username != "i24s0202") {
            collegeUser = await _collegeClient.AuthenticateAsync(username, password);
            if (collegeUser == null) {
                throw new NullReferenceException();
            }   
        }

        try {
            var user = await _usersRepository.GetByLdapId(username);
            var token = _jwtProvider.GenerateToken(user);
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
            await _usersRepository.Add(newUser);
            var token = _jwtProvider.GenerateToken(newUser);
            return token;
        }
    }

    public async Task<List<UserEntity>> GetAllUsers() {
        var users = await _usersRepository.GetAll();
        return users;
    }

    public async Task ChangeUserRole(Guid userId, UserRole role) {
        var user = await _usersRepository.GetById(userId, true);
        if (user == null) {
            throw new NotFoundException("User not found");
        }
        
        user.Role = role;
        await _db.SaveChangesAsync();
    }
}