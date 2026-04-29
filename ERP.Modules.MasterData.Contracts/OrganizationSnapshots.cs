namespace ERP.Modules.MasterData.Contracts;

public sealed record OrganizationSnapshot(
    long Id,
    Guid PublicId,
    OrganizationType AccountType,
    string? DisplayName);

public sealed record OrganizationCreateRequest(
    OrganizationType AccountType,
    string? DisplayName);
