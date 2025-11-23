using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services;

public class BillingService {
    private readonly ApplicationDbContext _dbContext;
    private readonly IUsersRepository _usersRepository;
    
    public BillingService(ApplicationDbContext dbContext,  IUsersRepository usersRepository) {
        _dbContext = dbContext;
        _usersRepository = usersRepository;
    }

    public async Task ConsumeTokensAsync(Guid userId, int count) {
        try {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user is null) {
                throw new NullReferenceException("User not found");
            }
            user.ConsumeTokens(count);
            await _dbContext.SaveChangesAsync();
        }
        catch (InvalidOperationException ex) {
            throw new BillingException(ex.Message);
        }
    }
    
    public async Task CreditTokensAsync(Guid creditorId, Guid userId, int count) {
        try {
            var creditor = await _dbContext.Users.FindAsync(creditorId);
            if (creditor is null) {
                throw new NullReferenceException("Creditor not found");
            }
            if (creditor.Role is UserRole.Teacher or UserRole.Admin) {
                var user = await _dbContext.Users.FindAsync(userId);
                if (user is null) {
                    throw new NullReferenceException("User not found");
                }
                
                if (user.Role is UserRole.Student) {
                    user.AddTokens(count);
                    await _dbContext.SaveChangesAsync();
                }
                
                throw new UserAccessException(
                    $"{user.Username} do not have permission to charge tokens to the {creditor.Role.ToString()}");
            }

            throw new UserAccessException($"{creditor.Username} do not have permission to charge tokens");
        }
        catch (InvalidOperationException ex) {
            throw new BillingException(ex.Message);
        }
    }
}