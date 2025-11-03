using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Infrastructure;
using Neur.Server.Net.Infrastructure.Interfaces;

namespace Neur.Server.Net.Application.Services;

public class UserService : IUserService {
    private readonly IUserRepository _userRepository;
    private readonly ILdapService _ldapService;
    private readonly IJwtProvider _jwtProvider;
    
    public UserService(IUserRepository userRepository, ILdapService ldapService,  IJwtProvider jwtProvider) {
        _userRepository = userRepository;
        _ldapService = ldapService;
        _jwtProvider = jwtProvider;
    }
    public async Task<string> Login(string username, string password) {
        var ldapUser = await _ldapService.AuthenticateAsync(username, password);
        if (ldapUser == null) {
            throw new Exception();
        }
        
        var user = await _userRepository.GetByLdapId(username);
        if (user == null) {
            var newUser = UserEntity.Create(
                id: Guid.NewGuid(),
                username: ldapUser.Username,
                name: ldapUser.Name,
                surname: ldapUser.Surname
            );
            await _userRepository.Add(newUser);
            user = newUser;
        }
        var token = _jwtProvider.GenerateToken(user);
        return token;
    }
}