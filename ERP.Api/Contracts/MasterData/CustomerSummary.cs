namespace ERP.Api.Contracts.MasterData;

public sealed record CustomerSummary(
    long Id,
    string Name,
    string? TaxId);
