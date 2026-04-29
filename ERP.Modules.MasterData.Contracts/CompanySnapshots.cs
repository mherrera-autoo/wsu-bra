namespace ERP.Modules.MasterData.Contracts;

public sealed record CompanySnapshot(
    long Id,
    Guid PublicId,
    long OrganizationId,
    long TaxEntityId,
    string Name);

public sealed record CompanyCreateRequest(
    long OrganizationId,
    long TaxEntityId,
    string Name);
