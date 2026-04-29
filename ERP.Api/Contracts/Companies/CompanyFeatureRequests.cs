namespace ERP.Api.Contracts.Companies;

public sealed record CompanyFeatureChangeRequest(string? Reason, string? CorrelationId);

public sealed record ReplaceCompanyFeaturesRequest(
    IReadOnlyCollection<string> Features,
    string? Reason,
    string? CorrelationId);

public sealed record CompanyFeatureSummary(string FeatureKey, DateTime EnabledAt);
