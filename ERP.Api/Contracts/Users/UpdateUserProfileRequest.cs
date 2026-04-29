namespace ERP.Api.Contracts.Users;

public sealed record UpdateUserProfileRequest(
    string Email,
    string FirstName,
    string LastName,
    string? DisplayName,
    bool IsActive);
