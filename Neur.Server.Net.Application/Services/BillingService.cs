using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Records;
using Neur.Server.Net.Core.Repositories;

namespace Neur.Server.Net.Application.Services;

public class BillingService {
    private readonly IUsersRepository _repository;
    
    public BillingService(IUsersRepository repository) {
        _repository = repository;
    }

    public async Task ConsumeTokensAsync(Guid userId, int count) {
        var user = await _repository.GetById(userId);
        user.ConsumeTokens(count);
        await _repository.Update(user);
    }
    
    public async Task CreditTokensAsync(Guid creditorId, Guid userId, int count) {
        var creditor = await _repository.GetById(creditorId);
        if (creditor.Role is UserRole.Teacher or UserRole.Admin) {
            var user = await _repository.GetById(userId);
            if (user.Role is UserRole.Student) {
                user.Tokens += count;
                await _repository.Update(user);
            }
            throw new UserAccessException($"{user.Username} do not have permission to charge tokens to the {creditor.Role.ToString()}");
        }
        throw new UserAccessException($"{creditor.Username} do not have permission to charge tokens");
    }
}