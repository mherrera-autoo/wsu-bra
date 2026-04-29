namespace ERP.Api.Contracts.Users;

public sealed record CreateUserProfileRequest(
    string Email,
    string FirstName,
    string LastName,
    string? DisplayName);
