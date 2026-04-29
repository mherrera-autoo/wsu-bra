namespace ERP.Api.Contracts.Identity;

public sealed record UserPermissionsResponse(
    string[] Platform,
    string[] Organization,
    string[] Company,
    UserPermissionsContext Context);

public sealed record UserPermissionsContext(
    Guid? CompanyPublicId,
    long? OrganizationId,
    bool HasCompanyMembership);