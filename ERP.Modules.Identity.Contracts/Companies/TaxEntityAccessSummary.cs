
namespace ERP.Modules.Identity.Contracts;

public sealed record TaxEntityAccessSummary(
    Guid TaxEntityPublicId,
    long TaxEntityId,
    string TaxId,
    string? DisplayName,
    bool HasOperableCompany,
    int CompanyCount,
    CompanyAccessSource SourceFlags,
    CompanyLinkAccessType? AccessType,
    Guid CompanyPublicId);
