using System.Security.Claims;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services;

public class TokenService : ITokenService {
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUsersRepository _usersRepository;
    
    public TokenService(IUnitOfWork unitOfWork, IUsersRepository usersRepository) {
        _unitOfWork = unitOfWork;
        _usersRepository = usersRepository;
    }

    public async Task GiveTokens(Guid ownerId, Guid userId, int tokenCount) {
        var owner = await _usersRepository.GetByIdAsync(ownerId, true) ?? throw new NotFoundException("Owner not found");
        var user = await _usersRepository.GetByIdAsync(userId, true) ??  throw new NotFoundException("User not found");
        
        if (owner.Tokens < tokenCount || tokenCount <= 0) {
            throw new BillingException();
        }
        
        owner.Tokens -= tokenCount;
        user.Tokens += tokenCount;

        await _unitOfWork.SaveChangesAsync();
    }
}