using Application.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> action,
        CancellationToken cancellationToken = default)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));

        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await action(cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<T>> action,
        CancellationToken cancellationToken = default)
    {
        if (action is null) throw new ArgumentNullException(nameof(action));

        await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            return result;
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }
    }
}