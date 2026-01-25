using System.Security.Claims;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services;

public class TokenService : ITokenService {
    private readonly ApplicationDbContext _context;
    
    public TokenService(ApplicationDbContext context) {
        _context = context;
    }

    public async Task GiveTokens(Guid ownerId, Guid userId, int tokenCount) {
        var owner = await _context.Users.FindAsync(ownerId);
        var user = await _context.Users.FindAsync(userId);
        if (owner == null) {
            throw new NotFoundException("Owner not found");
        }
        if (user == null) {
            throw new NotFoundException("User not found");
        }
        if (owner.Tokens < tokenCount || tokenCount <= 0) {
            throw new BillingException();
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try {
            owner.Tokens -= tokenCount;
            user.Tokens += tokenCount;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e) {
            await transaction.RollbackAsync();
            throw;
        }
    }
}