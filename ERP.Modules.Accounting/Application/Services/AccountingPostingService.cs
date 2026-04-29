using System.Text.Json;
using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using ERP.Modules.Integrations.Contracts;
using ERP.Shared.Application;
using ERP.Shared.Application.Events;
using ERP.Modules.Accounting.Contracts;

namespace ERP.Modules.Accounting.Application.Services;

public sealed class AccountingPostingService
{
    private const decimal BaseCurrencyFxRate = 1m;
    private const int BaseCurrencyScale = 2;

    private readonly ITenantContext _tenantContext;
    private readonly ICompanyAccountingSettingsRepository _settingsRepository;
    private readonly IAccountingPeriodRepository _periodRepository;
    private readonly IJournalRepository _journalRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AccountingPostingService(
        ITenantContext tenantContext,
        ICompanyAccountingSettingsRepository settingsRepository,
        IAccountingPeriodRepository periodRepository,
        IJournalRepository journalRepository,
        IAccountRepository accountRepository,
        IJournalEntryRepository journalEntryRepository,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantContext = tenantContext;
        _settingsRepository = settingsRepository;
        _periodRepository = periodRepository;
        _journalRepository = journalRepository;
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<JournalEntry>> PostAsync(AccountingPostRequested request, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var companyId = _tenantContext.CompanyId ?? 0;
            var userId = _tenantContext.UserId;
            if (companyId <= 0)
            {
                return Result<JournalEntry>.Fail("Tenant context is missing CompanyId.");
            }

            if (request.Lines.Count == 0)
            {
                return Result<JournalEntry>.Fail("Journal entry must contain at least one line.");
            }

            var existing = await _journalEntryRepository.GetBySourceAsync(
                companyId,
                request.SourceModule,
                request.SourceDocumentId,
                request.SourceDocumentType,
                token);
            if (existing is not null)
            {
                return Result<JournalEntry>.Ok(existing);
            }

            var settings = await _settingsRepository.GetAsync(companyId, token);
            if (settings is null)
            {
                return Result<JournalEntry>.Fail("Company accounting settings are missing.");
            }

            var period = await _periodRepository.GetOpenByDateAsync(companyId, request.EntryDate, token);
            if (period is null)
            {
                return Result<JournalEntry>.Fail("Accounting period is closed or missing.");
            }

            var journal = await _journalRepository.GetByCodeAsync(companyId, request.JournalCode, token);
            if (journal is null || !journal.IsActive)
            {
                return Result<JournalEntry>.Fail("Journal is missing or inactive.");
            }

            var accountIds = request.Lines.Select(line => line.AccountId).Distinct().ToList();
            var accounts = await _accountRepository.GetByIdsAsync(companyId, accountIds, token);
            if (accounts.Count != accountIds.Count)
            {
                return Result<JournalEntry>.Fail("One or more accounts are invalid.");
            }

            if (accounts.Any(account => !account.IsActive || !account.IsPostable))
            {
                return Result<JournalEntry>.Fail("One or more accounts are not active or postable.");
            }

            var postingDate = DateTime.UtcNow;
            var postedAt = postingDate;
            var entry = JournalEntry.CreatePosted(
                companyId,
                journal.Id,
                request.EntryDate,
                postingDate,
                period.Id,
                request.Description,
                request.SourceModule,
                request.SourceDocumentId,
                request.SourceDocumentType,
                userId,
                postedAt,
                userId);

            var lineResults = BuildLines(entry.Id, companyId, settings.BaseCurrencyCode, request.Lines);
            if (!lineResults.Success)
            {
                return Result<JournalEntry>.Fail(lineResults.Error ?? "Invalid journal entry lines.");
            }

            foreach (var line in lineResults.Value!)
            {
                entry.AddLine(line);
            }

            await _journalEntryRepository.AddAsync(entry, token);

            var totals = CalculateBaseTotals(entry.Lines);
            var postedEvent = new JournalEntryPosted(
                entry.Id,
                entry.SourceModule,
                entry.SourceDocumentType,
                entry.SourceDocumentId,
                entry.PostingDate,
                entry.EntryDate,
                entry.PeriodId,
                totals.TotalDebitBase,
                totals.TotalCreditBase,
                request.CorrelationId);

            var payload = JsonSerializer.Serialize(postedEvent);
            await _outboxRepository.AddAsync(
                OutboxMessage.Create("accounting.journal-entry.posted", payload),
                token);

            return Result<JournalEntry>.Ok(entry);
        }, cancellationToken);
    }

    public Task<Result<JournalEntry>> ReverseAsync(AccountingReverseRequested request, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var companyId = _tenantContext.CompanyId ?? 0;
            var userId = _tenantContext.UserId;
            if (companyId <= 0)
            {
                return Result<JournalEntry>.Fail("Tenant context is missing CompanyId.");
            }

            var existing = await _journalEntryRepository.GetBySourceAsync(
                companyId,
                "Accounting",
                request.OriginalJournalEntryId.ToString(),
                "Reversal",
                token);
            if (existing is not null)
            {
                return Result<JournalEntry>.Ok(existing);
            }

            var original = await _journalEntryRepository.GetByIdAsync(companyId, request.OriginalJournalEntryId, token);
            if (original is null)
            {
                return Result<JournalEntry>.Fail("Original journal entry not found.");
            }

            var settings = await _settingsRepository.GetAsync(companyId, token);
            if (settings is null)
            {
                return Result<JournalEntry>.Fail("Company accounting settings are missing.");
            }

            var period = await _periodRepository.GetOpenByDateAsync(companyId, DateTime.UtcNow.Date, token);
            if (period is null)
            {
                return Result<JournalEntry>.Fail("Accounting period is closed or missing.");
            }

            var entry = JournalEntry.CreatePosted(
                companyId,
                original.JournalId,
                original.EntryDate,
                DateTime.UtcNow,
                period.Id,
                request.Description ?? $"Reversal of entry {original.Id}",
                "Accounting",
                original.Id.ToString(),
                "Reversal",
                userId,
                DateTime.UtcNow,
                userId,
                original.Id);

            foreach (var line in original.Lines)
            {
                entry.AddLine(JournalEntryLine.Create(
                    companyId,
                    entry.Id,
                    line.AccountId,
                    line.Credit,
                    line.Debit,
                    line.CurrencyCode,
                    line.FxRate,
                    line.AmountInBaseCurrency,
                    line.CostCenterId,
                    line.ProjectId));
            }

            await _journalEntryRepository.AddAsync(entry, token);

            var totals = CalculateBaseTotals(entry.Lines);
            var postedEvent = new JournalEntryPosted(
                entry.Id,
                entry.SourceModule,
                entry.SourceDocumentType,
                entry.SourceDocumentId,
                entry.PostingDate,
                entry.EntryDate,
                entry.PeriodId,
                totals.TotalDebitBase,
                totals.TotalCreditBase,
                request.CorrelationId);

            var payload = JsonSerializer.Serialize(postedEvent);
            await _outboxRepository.AddAsync(
                OutboxMessage.Create("accounting.journal-entry.posted", payload),
                token);

            return Result<JournalEntry>.Ok(entry);
        }, cancellationToken);
    }

    private static Result<List<JournalEntryLine>> BuildLines(
        long journalEntryId,
        long companyId,
        string baseCurrencyCode,
        IReadOnlyList<AccountingPostRequestedLine> requestedLines)
    {
        var lines = new List<JournalEntryLine>(requestedLines.Count);
        foreach (var line in requestedLines)
        {
            if (line.Debit < 0 || line.Credit < 0)
            {
                return Result<List<JournalEntryLine>>.Fail("Debit/Credit must be >= 0.");
            }

            if ((line.Debit > 0 && line.Credit > 0) || (line.Debit == 0 && line.Credit == 0))
            {
                return Result<List<JournalEntryLine>>.Fail("Each line must contain either a debit or a credit.");
            }

            var currencyCode = string.IsNullOrWhiteSpace(line.CurrencyCode)
                ? baseCurrencyCode
                : line.CurrencyCode.Trim().ToUpperInvariant();

            var fxRate = currencyCode == baseCurrencyCode
                ? BaseCurrencyFxRate
                : line.FxRate ?? 0m;

            if (fxRate <= 0)
            {
                return Result<List<JournalEntryLine>>.Fail("FX rate must be provided for non-base currency lines.");
            }

            var debitBase = RoundBase(line.Debit * fxRate);
            var creditBase = RoundBase(line.Credit * fxRate);
            var amountBase = debitBase + creditBase;

            lines.Add(JournalEntryLine.Create(
                companyId,
                journalEntryId,
                line.AccountId,
                line.Debit,
                line.Credit,
                currencyCode,
                fxRate,
                amountBase,
                null,
                null));
        }

        var totals = CalculateBaseTotals(lines);
        if (totals.TotalDebitBase != totals.TotalCreditBase)
        {
            return Result<List<JournalEntryLine>>.Fail("Journal entry is not balanced in base currency.");
        }

        return Result<List<JournalEntryLine>>.Ok(lines);
    }

    private static (decimal TotalDebitBase, decimal TotalCreditBase) CalculateBaseTotals(IEnumerable<JournalEntryLine> lines)
    {
        var totalDebit = lines.Sum(line => RoundBase(line.Debit * line.FxRate));
        var totalCredit = lines.Sum(line => RoundBase(line.Credit * line.FxRate));
        return (totalDebit, totalCredit);
    }

    private static decimal RoundBase(decimal value)
        => Math.Round(value, BaseCurrencyScale, MidpointRounding.AwayFromZero);
}
