namespace ERP.Shared.Application;

public sealed record CurrentUser(
    long UserId,
    long OrganizationId,
    string Scope,
    Guid? CompanyPublicId,
    long CompanyId,
    string Email,
    IReadOnlyCollection<string> Roles);
