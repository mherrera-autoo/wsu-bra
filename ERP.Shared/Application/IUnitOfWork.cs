namespace ERP.Shared.Application;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> action, CancellationToken cancellationToken = default);
    Task<Result<T>> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<Result<T>>> action, CancellationToken cancellationToken = default);
}
