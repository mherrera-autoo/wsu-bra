namespace ERP.Api.Contracts.Companies;

public sealed record CompanyWithAssignmentSummary(
    long Id,
    Guid PublicId,
    long OrganizationId,
    long TaxEntityId,
    string Name,
    bool IsAssignedToUser);
