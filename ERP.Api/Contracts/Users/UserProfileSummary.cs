namespace ERP.Api.Contracts.Users;

public sealed record UserProfileSummary(
    long Id,
    string Email,
    string FirstName,
    string LastName,
    string? DisplayName,
    bool IsActive);
