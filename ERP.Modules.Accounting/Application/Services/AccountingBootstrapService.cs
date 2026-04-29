using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Accounting.Application.Services;

public sealed class AccountingBootstrapService
{
    private const string DefaultChartVersion = "CL_BASIC";
    private readonly ICompanyAccountingSettingsRepository _settingsRepository;
    private readonly IJournalRepository _journalRepository;
    private readonly IAccountingPeriodRepository _periodRepository;
    private readonly AccountingChartSeedService _chartSeedService;
    private readonly IUnitOfWork _unitOfWork;

    public AccountingBootstrapService(
        ICompanyAccountingSettingsRepository settingsRepository,
        IJournalRepository journalRepository,
        IAccountingPeriodRepository periodRepository,
        AccountingChartSeedService chartSeedService,
        IUnitOfWork unitOfWork)
    {
        _settingsRepository = settingsRepository;
        _journalRepository = journalRepository;
        _periodRepository = periodRepository;
        _chartSeedService = chartSeedService;
        _unitOfWork = unitOfWork;
    }

    public Task<Result> EnsureSeededAsync(long companyId, string baseCurrencyCode, CancellationToken cancellationToken = default)
    {
        return EnsureSeededInternalAsync(companyId, baseCurrencyCode, cancellationToken);
    }

    private async Task<Result> EnsureSeededInternalAsync(long companyId, string baseCurrencyCode, CancellationToken cancellationToken)
    {
        var templateResult = await _chartSeedService.EnsureGlobalTemplateAsync(DefaultChartVersion, cancellationToken);
        if (!templateResult.Success)
        {
            return templateResult;
        }

        return await EnsureCompanySeededAsync(companyId, baseCurrencyCode, cancellationToken);
    }

    public Task<Result> EnsureCompanySeededAsync(long companyId, string baseCurrencyCode, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var chartResult = await _chartSeedService.EnsureCompanyChartOfAccountsAsync(companyId, DefaultChartVersion, token);
            if (!chartResult.Success)
            {
                return chartResult;
            }

            var settings = await _settingsRepository.GetAsync(companyId, token);
            if (settings is null)
            {
                await _settingsRepository.AddAsync(
                    CompanyAccountingSettings.Create(companyId, baseCurrencyCode),
                    token);
            }

            var journals = new[]
            {
                ("SALES", "Sales Journal"),
                ("PURCHASES", "Purchases Journal"),
                ("CASH", "Cash Journal"),
                ("BANK", "Bank Journal"),
                ("GENERAL", "General Journal")
            };

            foreach (var (code, name) in journals)
            {
                var existing = await _journalRepository.GetByCodeAsync(companyId, code, token);
                if (existing is null)
                {
                    await _journalRepository.AddAsync(Journal.Create(companyId, code, name), token);
                }
            }

            var now = DateTime.UtcNow;
            var existingPeriod = await _periodRepository.GetByMonthAsync(companyId, now.Year, now.Month, token);
            if (existingPeriod is null)
            {
                await _periodRepository.AddAsync(AccountingPeriod.Open(companyId, now.Year, now.Month), token);
            }

            return Result.Ok();
        }, cancellationToken);
    }
}
