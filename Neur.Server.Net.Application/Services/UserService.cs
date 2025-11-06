using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Interfaces;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;
using Neur.Server.Net.Infrastructure.Interfaces;

namespace Neur.Server.Net.Application.Services;

public class UserService : IUserService {
    private readonly IUserRepository _userRepository;
    private readonly ICollegeService _collegeService;
    private readonly IJwtProvider _jwtProvider;
    
    public UserService(IUserRepository userRepository, ICollegeService collegeService,  IJwtProvider jwtProvider) {
        _userRepository = userRepository;
        _collegeService = collegeService;
        _jwtProvider = jwtProvider;
    }

    //Тут временный говнокод
    private Role DeterminateRole(string role) {
        switch (role) {
            case "student":
                return Role.Student;
            case "teacher":
                return Role.Teacher;
            case "admin":
                return Role.Admin;
        }
        throw new Exception("Role doesn't exist");
    }
    public async Task<string> Login(string username, string password) {
        var collegeUser = await _collegeService.AuthenticateAsync(username, password);
        if (collegeUser == null) {
            throw new NullReferenceException();
        }
        
        var user = await _userRepository.GetByLdapId(username);
        if (user == null) {
            //Говнокод
            var name = collegeUser.username.Split()[0];
            var surname = collegeUser.username.Split()[1];
            
            var newUser = UserEntity.Create(
                id: Guid.NewGuid(),
                username: collegeUser.id,
                name: name,
                surname: surname,
                role: DeterminateRole(collegeUser.role),
                tokens: 10
            );
            await _userRepository.Add(newUser);
            user = newUser;
        }
        var token = _jwtProvider.GenerateToken(user);
        return token;
    }
}