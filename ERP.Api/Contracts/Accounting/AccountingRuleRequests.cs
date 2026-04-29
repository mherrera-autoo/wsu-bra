using ERP.Modules.Accounting.Domain;

namespace ERP.Api.Contracts.Accounting;

public sealed record AccountingRuleRequest(
    string Name,
    AccountingRuleTrigger Trigger,
    DteDocumentType? DteDocumentType,
    string? SourceModule,
    string? SourceDocumentType,
    string? CounterpartyTaxId,
    long DebitAccountId,
    long CreditAccountId,
    string? TaxCode);

public sealed record AccountingRuleResponse(
    long Id,
    string Name,
    AccountingRuleTrigger Trigger,
    DteDocumentType? DteDocumentType,
    string? SourceModule,
    string? SourceDocumentType,
    string? CounterpartyTaxId,
    long DebitAccountId,
    long CreditAccountId,
    string? TaxCode,
    bool IsActive,
    bool ProposedBySystem);

public sealed record RuleProposalRequest(
    DteDocumentType? DteDocumentType,
    string? SourceModule,
    string? SourceDocumentType);
