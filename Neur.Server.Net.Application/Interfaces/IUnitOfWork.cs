namespace Neur.Server.Net.Application.Interfaces;

public interface IUnitOfWork {
    public Task SaveChangesAsync(CancellationToken cancellationToken = default);
}