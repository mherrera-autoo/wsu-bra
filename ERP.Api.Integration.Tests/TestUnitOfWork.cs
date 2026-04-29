using ERP.Persistence;
using ERP.Shared.Application;

namespace ERP.Api.Integration.Tests;

public sealed class TestUnitOfWork : IUnitOfWork
{
    private readonly ErpDbContext _dbContext;

    public TestUnitOfWork(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public async Task<Result> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<Result>> action,
        CancellationToken cancellationToken = default)
    {
        var result = await action(cancellationToken);
        if (!result.Success)
        {
            return result;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<Result<T>> ExecuteInTransactionAsync<T>(
        Func<CancellationToken, Task<Result<T>>> action,
        CancellationToken cancellationToken = default)
    {
        var result = await action(cancellationToken);
        if (!result.Success)
        {
            return result;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return result;
    }
}
