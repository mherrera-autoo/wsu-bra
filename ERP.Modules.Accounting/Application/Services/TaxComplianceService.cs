using System.Text.Json;
using ERP.Documents.Contracts;
using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using ERP.Modules.Integrations.Contracts;
using ERP.Shared.Application;

namespace ERP.Modules.Accounting.Application.Services;

public sealed class TaxComplianceService
{
    private readonly IDteDocumentRepository _dteRepository;
    private readonly ITaxBookEntryRepository _taxBookRepository;
    private readonly ITaxDeclarationRepository _taxDeclarationRepository;
    private readonly IAccountingAutomationRuleRepository _ruleRepository;
    private readonly AccountingAutomationEngine _automationEngine;
    private readonly IAutomationProviderRegistry _automationProviderRegistry;
    private readonly IOutboxRepository _outboxRepository;

    public TaxComplianceService(
        IDteDocumentRepository dteRepository,
        ITaxBookEntryRepository taxBookRepository,
        ITaxDeclarationRepository taxDeclarationRepository,
        IAccountingAutomationRuleRepository ruleRepository,
        AccountingAutomationEngine automationEngine,
        IAutomationProviderRegistry automationProviderRegistry,
        IOutboxRepository outboxRepository)
    {
        _dteRepository = dteRepository;
        _taxBookRepository = taxBookRepository;
        _taxDeclarationRepository = taxDeclarationRepository;
        _ruleRepository = ruleRepository;
        _automationEngine = automationEngine;
        _automationProviderRegistry = automationProviderRegistry;
        _outboxRepository = outboxRepository;
    }

    public async Task<Result<DteDocument>> IngestDteAsync(
        long companyId,
        string folio,
        DteDocumentType documentType,
        DateTime issueDate,
        string counterpartyTaxId,
        decimal netAmount,
        decimal taxAmount,
        decimal totalAmount,
        string currencyCode,
        string source,
        string? automationProviderKey,
        CancellationToken cancellationToken = default)
    {
        var existing = await _dteRepository.GetByFolioAsync(companyId, folio, documentType, cancellationToken);
        if (existing is not null)
        {
            return Result<DteDocument>.Fail("DTE already ingested.");
        }

        var document = DteDocument.Create(
            companyId,
            folio,
            documentType,
            issueDate,
            counterpartyTaxId,
            netAmount,
            taxAmount,
            totalAmount,
            currencyCode,
            source);

        await _dteRepository.AddAsync(document, cancellationToken);
        await EnqueueDocumentUploadedAsync(companyId, document, cancellationToken);
        if (!string.IsNullOrWhiteSpace(automationProviderKey))
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(document);
            var dispatchRequest = new AutomationRequest(companyId, automationProviderKey, "dte-ingest", payload);
            var dispatchResult = await _automationProviderRegistry.DispatchAsync(
                companyId,
                automationProviderKey,
                dispatchRequest,
                cancellationToken);
            if (!dispatchResult.Success)
            {
                return Result<DteDocument>.Fail(dispatchResult.Message ?? "Automation provider rejected the DTE.");
            }
        }

        var automationResult = await _automationEngine.ApplyDteRulesAsync(document, cancellationToken);
        if (!automationResult.Success)
        {
            return Result<DteDocument>.Fail(automationResult.Error ?? "Failed to auto-post DTE.");
        }

        return Result<DteDocument>.Ok(document);
    }

    private async Task EnqueueDocumentUploadedAsync(long companyId, DteDocument document, CancellationToken cancellationToken)
    {
        var uploaded = new DocumentUploaded(
            companyId,
            document.DocumentType.ToString(),
            document.Folio,
            "accounting.tax-compliance",
            DateTime.UtcNow);
        var payload = JsonSerializer.Serialize(uploaded);
        await _outboxRepository.AddAsync(OutboxMessage.Create("documents.uploaded", payload), cancellationToken);
    }

    public Task<IReadOnlyList<DteDocument>> ListDtesAsync(long companyId, CancellationToken cancellationToken = default)
        => _dteRepository.ListByCompanyAsync(companyId, cancellationToken);

    public async Task<IReadOnlyList<TaxBookEntry>> GenerateTaxBookAsync(
        long companyId,
        int year,
        int month,
        TaxBookType bookType,
        string? automationProviderKey,
        CancellationToken cancellationToken = default)
    {
        var documents = await _dteRepository.ListByPeriodAsync(companyId, year, month, cancellationToken);
        var filtered = documents
            .Where(document => bookType == TaxBookType.Sales
                ? document.DocumentType is DteDocumentType.Invoice or DteDocumentType.CreditNote or DteDocumentType.DebitNote or DteDocumentType.ExportInvoice
                : document.DocumentType is DteDocumentType.PurchaseInvoice or DteDocumentType.PurchaseCreditNote)
            .ToList();

        var entries = filtered
            .Select(document => TaxBookEntry.Create(companyId, year, month, bookType, document))
            .ToList();

        if (entries.Count > 0)
        {
            await _taxBookRepository.AddRangeAsync(entries, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(automationProviderKey))
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(entries);
            var dispatchRequest = new AutomationRequest(companyId, automationProviderKey, "tax-book-generate", payload);
            var dispatchResult = await _automationProviderRegistry.DispatchAsync(
                companyId,
                automationProviderKey,
                dispatchRequest,
                cancellationToken);
            _ = dispatchResult;
        }

        return entries;
    }

    public Task<IReadOnlyList<TaxBookEntry>> ListTaxBookEntriesAsync(
        long companyId,
        int year,
        int month,
        TaxBookType bookType,
        CancellationToken cancellationToken = default)
        => _taxBookRepository.ListByPeriodAsync(companyId, year, month, bookType, cancellationToken);

    public async Task<Result<TaxDeclaration>> CreateDeclarationAsync(
        long companyId,
        int year,
        int month,
        TaxDeclarationType declarationType,
        string payload,
        string? automationProviderKey,
        CancellationToken cancellationToken = default)
    {
        var declaration = TaxDeclaration.Create(companyId, year, month, declarationType, payload);
        await _taxDeclarationRepository.AddAsync(declaration, cancellationToken);

        if (!string.IsNullOrWhiteSpace(automationProviderKey))
        {
            var dispatchRequest = new AutomationRequest(
                companyId,
                automationProviderKey,
                "tax-declaration",
                payload);
            var result = await _automationProviderRegistry.DispatchAsync(companyId, automationProviderKey, dispatchRequest, cancellationToken);
            if (result.Success)
            {
                declaration.MarkSubmitted(result.ExternalReference, result.Message);
            }
            else
            {
                declaration.MarkRejected(result.Message ?? "Automation provider rejected the declaration.");
            }
        }

        return Result<TaxDeclaration>.Ok(declaration);
    }

    public Task<IReadOnlyList<TaxDeclaration>> ListDeclarationsAsync(long companyId, CancellationToken cancellationToken = default)
        => _taxDeclarationRepository.ListByCompanyAsync(companyId, cancellationToken);

    public async Task<Result<AccountingAutomationRule>> CreateRuleAsync(
        long companyId,
        string name,
        AccountingRuleTrigger trigger,
        DteDocumentType? dteDocumentType,
        string? sourceModule,
        string? sourceDocumentType,
        string? counterpartyTaxId,
        long debitAccountId,
        long creditAccountId,
        string? taxCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<AccountingAutomationRule>.Fail("Rule name is required.");
        }

        if (debitAccountId <= 0 || creditAccountId <= 0)
        {
            return Result<AccountingAutomationRule>.Fail("Debit and credit accounts are required.");
        }

        var rule = AccountingAutomationRule.CreateManual(
            companyId,
            name,
            trigger,
            dteDocumentType,
            sourceModule,
            sourceDocumentType,
            counterpartyTaxId,
            debitAccountId,
            creditAccountId,
            taxCode);

        await _ruleRepository.AddAsync(rule, cancellationToken);
        return Result<AccountingAutomationRule>.Ok(rule);
    }

    public Task<IReadOnlyList<AccountingAutomationRule>> ListRulesAsync(long companyId, CancellationToken cancellationToken = default)
        => _ruleRepository.ListByCompanyAsync(companyId, cancellationToken);

    public Task<IReadOnlyList<AccountingAutomationRule>> ProposeRulesAsync(
        long companyId,
        DteDocumentType? documentType,
        string? sourceModule,
        string? sourceDocumentType,
        CancellationToken cancellationToken = default)
        => _automationEngine.SuggestRulesAsync(companyId, documentType, sourceModule, sourceDocumentType, cancellationToken);
}
