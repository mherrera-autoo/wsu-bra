using ERP.Shared.Application;

namespace ERP.Modules.Inventory.WhiteBox.Tests;

public sealed class InMemoryUnitOfWork : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

    public Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> action, CancellationToken cancellationToken = default)
        => action(cancellationToken);

    public Task<Result<T>> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<Result<T>>> action, CancellationToken cancellationToken = default)
        => action(cancellationToken);
}
