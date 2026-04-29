namespace ERP.Modules.Identity.Contracts;

public sealed record UserSummary(
    long UserId,
    Guid CompanyPublicId,
    long CompanyId,
    string Email,
    bool IsActive);
