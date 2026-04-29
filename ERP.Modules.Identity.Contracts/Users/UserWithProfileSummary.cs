namespace ERP.Modules.Identity.Contracts;

public sealed record UserWithProfileSummary(
    Guid UserPublicId,
    string Email,
    string? FirstName,
    string? LastName,
    string? DisplayName);
