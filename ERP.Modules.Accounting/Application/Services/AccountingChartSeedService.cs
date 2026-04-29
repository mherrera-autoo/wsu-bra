using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Accounting.Application.Services;

public sealed class AccountingChartSeedService
{
    private readonly IAccountingAccountTemplateRepository _templateRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IChartOfAccountsTemplateSource _templateSource;
    private readonly IUnitOfWork _unitOfWork;

    public AccountingChartSeedService(
        IAccountingAccountTemplateRepository templateRepository,
        IAccountRepository accountRepository,
        IChartOfAccountsTemplateSource templateSource,
        IUnitOfWork unitOfWork)
    {
        _templateRepository = templateRepository;
        _accountRepository = accountRepository;
        _templateSource = templateSource;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> EnsureGlobalTemplateAsync(string version, CancellationToken cancellationToken = default)
    {
        var entries = await _templateSource.LoadAsync(version, cancellationToken);
        if (entries.Count == 0)
        {
            return Result.Fail($"Chart of accounts template '{version}' has no entries.");
        }

        var existing = await _templateRepository.ListByVersionAsync(version, cancellationToken);
        var templateMap = existing.ToDictionary(template => template.Code, StringComparer.OrdinalIgnoreCase);
        var entryMap = new Dictionary<string, AccountingAccountTemplate>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in entries)
        {
            var isSystemRequired = entry.IsSystemRequired ?? false;
            var isActive = entry.IsActive ?? true;

            if (templateMap.TryGetValue(entry.Code, out var template))
            {
                template.UpdateDetails(
                    entry.Name,
                    ParseAccountType(entry.AccountType),
                    isSystemRequired,
                    isActive,
                    entry.SortOrder,
                    entry.SystemRole);
            }
            else
            {
                var created = AccountingAccountTemplate.Create(
                    version,
                    entry.Code,
                    entry.Name,
                    ParseAccountType(entry.AccountType),
                    isSystemRequired,
                    isActive,
                    entry.SortOrder,
                    entry.SystemRole);
                await _templateRepository.AddAsync(created, cancellationToken);
                templateMap[created.Code] = created;
                template = created;
            }

            entryMap[entry.Id] = template;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var entry in entries)
        {
            var template = entryMap[entry.Id];
            var parentId = entry.ParentTemplateAccountId is null
                ? null
                : entryMap.TryGetValue(entry.ParentTemplateAccountId, out var parent)
                    ? (long?)parent.Id
                    : null;

            if (template.ParentId != parentId)
            {
                template.SetParent(parentId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> EnsureCompanyChartOfAccountsAsync(
        long companyId,
        string version,
        CancellationToken cancellationToken = default)
        => await SeedCompanyChartOfAccountsFromGlobalTemplateAsync(companyId, version, cancellationToken);

    public Task<Result> SeedCompanyChartOfAccountsFromGlobalTemplateAsync(
        long companyId,
        string version,
        CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var existingCount = await _accountRepository.CountAsync(companyId, token);
            if (existingCount > 0)
            {
                return Result.Ok();
            }

            var templates = await _templateRepository.ListByVersionAsync(version, token);
            if (templates.Count == 0)
            {
                return Result.Fail($"Chart of accounts template '{version}' not found.");
            }

            var rootTemplates = templates
                .Where(template => !template.ParentId.HasValue)
                .OrderBy(template => template.SortOrder)
                .ThenBy(template => template.Code)
                .ToList();
            if (rootTemplates.Count == 0)
            {
                return Result.Fail($"Chart of accounts template '{version}' has no root accounts.");
            }

            var accountMap = new Dictionary<long, Account>();
            foreach (var template in rootTemplates)
            {
                var account = Account.Create(
                    companyId,
                    template.Code,
                    template.Name,
                    template.AccountType,
                    parentId: null,
                    isSystemRequired: template.IsSystemRequired,
                    isLocked: template.IsSystemRequired,
                    isActive: template.IsActive,
                    sortOrder: template.SortOrder,
                    templateId: template.Id,
                    systemRole: template.SystemRole,
                    sourceTemplateAccountPublicId: template.PublicId);

                await _accountRepository.AddAsync(account, token);
                accountMap[template.Id] = account;
            }

            await _unitOfWork.SaveChangesAsync(token);

            var pendingTemplates = templates
                .Where(template => template.ParentId.HasValue)
                .OrderBy(template => template.SortOrder)
                .ThenBy(template => template.Code)
                .ToList();

            while (pendingTemplates.Count > 0)
            {
                var readyTemplates = pendingTemplates
                    .Where(template => accountMap.ContainsKey(template.ParentId!.Value))
                    .ToList();

                if (readyTemplates.Count == 0)
                {
                    return Result.Fail($"Chart of accounts template '{version}' hierarchy could not be resolved.");
                }

                foreach (var template in readyTemplates)
                {
                    var parentAccount = accountMap[template.ParentId!.Value];
                    var account = Account.Create(
                        companyId,
                        template.Code,
                        template.Name,
                        template.AccountType,
                        parentId: parentAccount.Id,
                        isSystemRequired: template.IsSystemRequired,
                        isLocked: template.IsSystemRequired,
                        isActive: template.IsActive,
                        sortOrder: template.SortOrder,
                        templateId: template.Id,
                        systemRole: template.SystemRole,
                        sourceTemplateAccountPublicId: template.PublicId);

                    await _accountRepository.AddAsync(account, token);
                    accountMap[template.Id] = account;
                }

                await _unitOfWork.SaveChangesAsync(token);

                foreach (var template in readyTemplates)
                {
                    pendingTemplates.Remove(template);
                }
            }

            return Result.Ok();
        }, cancellationToken);
    }

    private static AccountType ParseAccountType(string accountType)
        => Enum.TryParse<AccountType>(accountType, true, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Invalid account type '{accountType}'.");
}
