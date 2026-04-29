namespace ERP.Api.Contracts.Onboarding;

public sealed record AddClientCompanyRequest(string TaxId, string? DisplayName);
