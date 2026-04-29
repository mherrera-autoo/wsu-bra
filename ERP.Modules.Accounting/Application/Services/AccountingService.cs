using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Accounting.Application.Services;

public sealed class AccountingService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AccountingService(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Account>> CreateAccountAsync(
        long companyId,
        string code,
        string name,
        AccountType accountType,
        long? parentId,
        bool isActive,
        int sortOrder,
        CancellationToken cancellationToken = default)
    {
        if (await _accountRepository.ExistsByCodeAsync(companyId, code, cancellationToken: cancellationToken))
        {
            return Result<Account>.Fail($"Account code '{code}' already exists.");
        }

        if (parentId.HasValue)
        {
            var parent = await _accountRepository.GetByIdAsync(companyId, parentId.Value, cancellationToken);
            if (parent is null)
            {
                return Result<Account>.Fail("Parent account not found.");
            }
        }

        var account = Account.Create(
            companyId,
            code,
            name,
            accountType,
            parentId,
            isSystemRequired: false,
            isLocked: false,
            isActive: isActive,
            sortOrder: sortOrder);
        await _accountRepository.AddAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Account>.Ok(account);
    }

    public Task<IReadOnlyList<Account>> ListAccountsAsync(
        long companyId,
        string? search,
        bool includeInactive,
        bool includeSystem,
        CancellationToken cancellationToken = default)
        => _accountRepository.ListAsync(companyId, search, includeInactive, includeSystem, cancellationToken);

    public Task<Account?> GetAccountAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _accountRepository.GetByIdAsync(companyId, id, cancellationToken);

    public async Task<Result<Account>> UpdateAccountAsync(
        long companyId,
        long id,
        string code,
        string name,
        AccountType accountType,
        long? parentId,
        bool isActive,
        int sortOrder,
        CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (account is null)
        {
            return Result<Account>.Fail("Account not found.");
        }

        if (account.IsLocked || account.IsSystemRequired)
        {
            if (!string.Equals(account.Code, code, StringComparison.OrdinalIgnoreCase)
                || account.AccountType != accountType
                || account.ParentId != parentId
                || account.SortOrder != sortOrder)
            {
                return Result<Account>.Fail("Account is locked.");
            }

            account.UpdateNameAndStatus(name, isActive);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Account>.Ok(account);
        }

        if (await _accountRepository.ExistsByCodeAsync(companyId, code, account.Id, cancellationToken))
        {
            return Result<Account>.Fail($"Account code '{code}' already exists.");
        }

        if (parentId.HasValue)
        {
            if (parentId.Value == account.Id)
            {
                return Result<Account>.Fail("Account cannot be its own parent.");
            }

            var parent = await _accountRepository.GetByIdAsync(companyId, parentId.Value, cancellationToken);
            if (parent is null)
            {
                return Result<Account>.Fail("Parent account not found.");
            }

            if (await CreatesCycleAsync(companyId, account.Id, parentId.Value, cancellationToken))
            {
                return Result<Account>.Fail("Account hierarchy cannot contain cycles.");
            }
        }

        account.UpdateDetails(code, name, accountType, parentId, isActive, sortOrder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Account>.Ok(account);
    }

    public async Task<Result> DeleteAccountAsync(long companyId, long id, CancellationToken cancellationToken = default)
    {
        var account = await _accountRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (account is null)
        {
            return Result.Fail("Account not found.");
        }

        if (account.IsLocked || account.IsSystemRequired)
        {
            return Result.Fail("Account is locked.");
        }

        if (await _accountRepository.HasChildrenAsync(companyId, account.Id, cancellationToken))
        {
            return Result.Fail("Account has child accounts.");
        }

        _accountRepository.Remove(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    private async Task<bool> CreatesCycleAsync(long companyId, long accountId, long newParentId, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.ListAsync(companyId, cancellationToken);
        var accountMap = accounts.ToDictionary(item => item.Id);

        var currentParent = newParentId;
        while (accountMap.TryGetValue(currentParent, out var parent))
        {
            if (parent.Id == accountId)
            {
                return true;
            }

            if (!parent.ParentId.HasValue)
            {
                break;
            }

            currentParent = parent.ParentId.Value;
        }

        return false;
    }
}
