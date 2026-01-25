using Microsoft.EntityFrameworkCore;
using Neur.Server.Net.Application.Exeptions;
using Neur.Server.Net.Core.Data;
using Neur.Server.Net.Core.Entities;
using Neur.Server.Net.Core.Repositories;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Application.Services;

public class GenerationRequestService {
    private readonly ApplicationDbContext _context;
    private readonly IGenerationRequestsRepository _requestsRepository;
    private readonly IUsersRepository _usersRepository;
    
    public GenerationRequestService(ApplicationDbContext context, IGenerationRequestsRepository requestsRepository, IUsersRepository usersRepository) {
        _context = context;
        _requestsRepository = requestsRepository;
        _usersRepository = usersRepository;
    }
    
    public async Task<IEnumerable<GenerationRequestEntity>> GetAllGenerationRequests(Guid userId) {
        var user = await _usersRepository.GetByIdAsync(userId);
        if (user == null) {
            throw new NotFoundException("User not found");
        }
        
        var requests = await _context.GenerationRequests
            .AsNoTracking()
            .Include(x => x.User)
            .Include(x => x.Model)
            .Where(x => x.User.Role <= user.Role)
            .ToListAsync();
        
        return requests;
    }
}