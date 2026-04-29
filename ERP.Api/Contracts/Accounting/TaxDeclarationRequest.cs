using ERP.Modules.Accounting.Domain;

namespace ERP.Api.Contracts.Accounting;

public sealed record TaxDeclarationRequest(
    int Year,
    int Month,
    TaxDeclarationType DeclarationType,
    string Payload,
    string? AutomationProviderKey);

public sealed record TaxDeclarationResponse(
    long Id,
    int Year,
    int Month,
    TaxDeclarationType DeclarationType,
    TaxDeclarationStatus Status,
    string Payload,
    string? ExternalReference,
    string? StatusMessage);
