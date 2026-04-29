namespace ERP.Modules.Identity.Contracts;

public sealed record CompanyTaxEntityInfo(
    long CompanyId,
    long TaxEntityId,
    Guid TaxEntityPublicId,
    string TaxId,
    string? DisplayName);
