using ERP.Shared.Application;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class ErpUnitOfWork : IUnitOfWork
{
    private readonly ErpDbContext _dbContext;

    public ErpUnitOfWork(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public async Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> action, CancellationToken cancellationToken = default)
    {
        var existingTransaction = _dbContext.Database.CurrentTransaction;
        if (existingTransaction is not null)
        {
            try
            {
                var result = await action(cancellationToken);
                if (!result.Success)
                {
                    return result;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result.Fail("Concurrency conflict while updating stock.");
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(cancellationToken);
            if (!result.Success)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Fail("Concurrency conflict while updating stock.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Fail(ex.Message);
        }
    }

    public async Task<Result<T>> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<Result<T>>> action, CancellationToken cancellationToken = default)
    {
        var existingTransaction = _dbContext.Database.CurrentTransaction;
        if (existingTransaction is not null)
        {
            try
            {
                var result = await action(cancellationToken);
                if (!result.Success)
                {
                    return result;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                return result;
            }
            catch (DbUpdateConcurrencyException)
            {
                return Result<T>.Fail("Concurrency conflict while updating stock.");
            }
            catch (Exception ex)
            {
                return Result<T>.Fail(ex.Message);
            }
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await action(cancellationToken);
            if (!result.Success)
            {
                await transaction.RollbackAsync(cancellationToken);
                return result;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<T>.Fail("Concurrency conflict while updating stock.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result<T>.Fail(ex.Message);
        }
    }
}
