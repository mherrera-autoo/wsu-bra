using ERP.Modules.Accounting.Domain;

namespace ERP.Api.Contracts.Accounting;

public sealed record DteDocumentRequest(
    string Folio,
    DteDocumentType DocumentType,
    DateTime IssueDate,
    string CounterpartyTaxId,
    decimal NetAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string CurrencyCode,
    string Source,
    string? AutomationProviderKey);

public sealed record DteDocumentResponse(
    long Id,
    string Folio,
    DteDocumentType DocumentType,
    DateTime IssueDate,
    string CounterpartyTaxId,
    decimal NetAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string CurrencyCode,
    DteDocumentStatus Status,
    string? StatusReason,
    string Source);
