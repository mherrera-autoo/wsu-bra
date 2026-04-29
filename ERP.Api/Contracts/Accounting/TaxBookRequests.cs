using ERP.Modules.Accounting.Domain;

namespace ERP.Api.Contracts.Accounting;

public sealed record TaxBookGenerateRequest(int Year, int Month, TaxBookType BookType, string? AutomationProviderKey);

public sealed record TaxBookEntryResponse(
    long Id,
    int Year,
    int Month,
    TaxBookType BookType,
    string Folio,
    DteDocumentType DocumentType,
    DateTime IssueDate,
    string CounterpartyTaxId,
    decimal NetAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    string CurrencyCode);
