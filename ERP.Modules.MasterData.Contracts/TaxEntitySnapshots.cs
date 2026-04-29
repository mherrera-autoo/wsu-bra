namespace ERP.Modules.MasterData.Contracts;

public sealed record TaxEntitySnapshot(
    long Id,
    Guid PublicId,
    Guid CompanyPublicId,
    string TaxId,
    string? DisplayName);

public sealed record TaxEntityCreateRequest(
    Guid CompanyPublicId,
    string TaxId,
    string? DisplayName);
