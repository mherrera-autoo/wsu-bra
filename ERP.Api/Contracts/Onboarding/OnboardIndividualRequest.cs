namespace ERP.Api.Contracts.Onboarding;

public sealed record OnboardIndividualRequest(string TaxId, string? DisplayName);
