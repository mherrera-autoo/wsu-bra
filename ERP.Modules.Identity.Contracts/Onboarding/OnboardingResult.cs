
namespace ERP.Modules.Identity.Contracts;

public sealed record OnboardingResult(
    long OrganizationId,
    Guid? CompanyPublicId,
    long? CompanyId,
    LoginResult? TenantToken,
    LoginResult? PlatformToken);

public sealed record CompanySelectionResult(
    long OrganizationId,
    Guid CompanyPublicId,
    long CompanyId,
    LoginResult TenantToken);

public sealed record ClientCompanyResult(
    long OrganizationId,
    Guid CompanyPublicId,
    long CompanyId);
