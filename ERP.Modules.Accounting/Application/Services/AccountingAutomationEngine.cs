using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Contracts;
using ERP.Modules.Accounting.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Accounting.Application.Services;

public sealed class AccountingAutomationEngine
{
    private readonly IAccountingAutomationRuleRepository _ruleRepository;
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly ICompanyAccountingSettingsRepository _settingsRepository;
    private readonly AccountingPostingService _postingService;

    public AccountingAutomationEngine(
        IAccountingAutomationRuleRepository ruleRepository,
        IJournalEntryRepository journalEntryRepository,
        ICompanyAccountingSettingsRepository settingsRepository,
        AccountingPostingService postingService)
    {
        _ruleRepository = ruleRepository;
        _journalEntryRepository = journalEntryRepository;
        _settingsRepository = settingsRepository;
        _postingService = postingService;
    }

    public async Task<Result<IReadOnlyList<JournalEntry>>> ApplyDteRulesAsync(
        DteDocument document,
        CancellationToken cancellationToken = default)
    {
        var rules = await _ruleRepository.ListByCompanyAsync(document.CompanyId, cancellationToken);
        var applicableRules = rules
            .Where(rule => rule.IsActive)
            .Where(rule => rule.Trigger == AccountingRuleTrigger.DteReceived)
            .Where(rule => rule.DteDocumentType is null || rule.DteDocumentType == document.DocumentType)
            .Where(rule => string.IsNullOrWhiteSpace(rule.CounterpartyTaxId)
                || string.Equals(rule.CounterpartyTaxId, document.CounterpartyTaxId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (applicableRules.Count == 0)
        {
            return Result<IReadOnlyList<JournalEntry>>.Ok(Array.Empty<JournalEntry>());
        }

        var settings = await _settingsRepository.GetAsync(document.CompanyId, cancellationToken);
        if (settings is null)
        {
            return Result<IReadOnlyList<JournalEntry>>.Fail("Company accounting settings are missing.");
        }

        var results = new List<JournalEntry>();
        foreach (var rule in applicableRules)
        {
            if (rule.DebitAccountId <= 0 || rule.CreditAccountId <= 0)
            {
                continue;
            }

            var journalCode = ResolveJournalCode(document.DocumentType, rule.SourceModule);
            var request = new AccountingPostRequested(
                Guid.NewGuid().ToString("N"),
                rule.SourceModule ?? "SII",
                rule.SourceDocumentType ?? document.DocumentType.ToString(),
                document.Id.ToString(),
                journalCode,
                document.IssueDate,
                rule.Name,
                new[]
                {
                    new AccountingPostRequestedLine(
                        rule.DebitAccountId,
                        document.TotalAmount,
                        0m,
                        settings.BaseCurrencyCode,
                        1m),
                    new AccountingPostRequestedLine(
                        rule.CreditAccountId,
                        0m,
                        document.TotalAmount,
                        settings.BaseCurrencyCode,
                        1m)
                });

            var result = await _postingService.PostAsync(request, cancellationToken);
            if (!result.Success)
            {
                return Result<IReadOnlyList<JournalEntry>>.Fail(result.Error ?? "Failed to auto-post DTE.");
            }

            results.Add(result.Value!);
        }

        return Result<IReadOnlyList<JournalEntry>>.Ok(results);
    }

    public async Task<IReadOnlyList<AccountingAutomationRule>> SuggestRulesAsync(
        long companyId,
        DteDocumentType? dteDocumentType,
        string? sourceModule,
        string? sourceDocumentType,
        CancellationToken cancellationToken = default)
    {
        var entries = await _journalEntryRepository.ListByCompanyAsync(companyId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(sourceModule))
        {
            entries = entries
                .Where(entry => string.Equals(entry.SourceModule, sourceModule, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var targetSourceDocumentType = string.IsNullOrWhiteSpace(sourceDocumentType)
            ? dteDocumentType?.ToString()
            : sourceDocumentType;

        if (!string.IsNullOrWhiteSpace(targetSourceDocumentType))
        {
            entries = entries
                .Where(entry => string.Equals(entry.SourceDocumentType, targetSourceDocumentType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var pairs = entries
            .Select(ExtractPair)
            .Where(pair => pair.HasValue)
            .Select(pair => pair.Value)
            .GroupBy(pair => new { pair.DebitAccountId, pair.CreditAccountId })
            .Select(group => new
            {
                group.Key.DebitAccountId,
                group.Key.CreditAccountId,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .Take(5)
            .ToList();

        var suggestions = new List<AccountingAutomationRule>();
        foreach (var pair in pairs)
        {
            var name = $"Suggested rule ({pair.Count} posts)";
            suggestions.Add(AccountingAutomationRule.CreateProposed(
                companyId,
                name,
                AccountingRuleTrigger.DteReceived,
                dteDocumentType,
                sourceModule,
                targetSourceDocumentType,
                null,
                pair.DebitAccountId,
                pair.CreditAccountId,
                null));
        }

        return suggestions;
    }

    private static (long DebitAccountId, long CreditAccountId)? ExtractPair(JournalEntry entry)
    {
        if (entry.Lines.Count == 0)
        {
            return null;
        }

        var debitLine = entry.Lines
            .OrderByDescending(line => line.Debit)
            .FirstOrDefault(line => line.Debit > 0);
        var creditLine = entry.Lines
            .OrderByDescending(line => line.Credit)
            .FirstOrDefault(line => line.Credit > 0);

        if (debitLine is null || creditLine is null)
        {
            return null;
        }

        return (DebitAccountId: debitLine.AccountId, CreditAccountId: creditLine.AccountId);
    }

    private static string ResolveJournalCode(DteDocumentType documentType, string? sourceModule)
    {
        return documentType switch
        {
            DteDocumentType.PurchaseInvoice or DteDocumentType.PurchaseCreditNote => "PURCHASES",
            DteDocumentType.Invoice or DteDocumentType.CreditNote or DteDocumentType.DebitNote or DteDocumentType.ExportInvoice => "SALES",
            _ => string.IsNullOrWhiteSpace(sourceModule) ? "GENERAL" : "GENERAL"
        };
    }
}
