namespace ERP.Api.Contracts.Identity;

public sealed record UpdateUserRequest(Guid CompanyPublicId, string? Email, string? Password, bool? IsActive);
