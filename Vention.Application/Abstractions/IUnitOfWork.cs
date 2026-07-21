using System.Data;

namespace Vention.Application.Abstractions
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
        Task BeginTransactionAsync(IsolationLevel level = IsolationLevel.ReadCommitted, CancellationToken ct = default);
        Task CommitTransactionAsync(CancellationToken ct = default);
        Task RollbackTransactionAsync(CancellationToken ct = default);

     
    }
}
