namespace ERP.Api.Contracts.Companies;

public sealed record CompanyFeaturesResponse(
    Guid CompanyPublicId,
    string[] EnabledFeatures);
