using Microsoft.EntityFrameworkCore.Storage;

namespace Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default);

    Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default);
}