using Neur.Server.Net.Application.Interfaces;
using Neur.Server.Net.Application.Interfaces.Services;
using Neur.Server.Net.Postgres;

namespace Neur.Server.Net.Infrastructure;

public class UnitOfWork : IUnitOfWork {
    private readonly ApplicationDbContext _context;
    
    public UnitOfWork(ApplicationDbContext dbContext) {
        _context = dbContext;
    }
    
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) {
        await _context.SaveChangesAsync(cancellationToken);
    }
}