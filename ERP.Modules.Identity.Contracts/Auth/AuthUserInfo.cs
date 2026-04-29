namespace ERP.Modules.Identity.Contracts;

public sealed record AuthUserInfo(
    long UserId,
    Guid? CompanyPublicId,
    long CompanyId,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
