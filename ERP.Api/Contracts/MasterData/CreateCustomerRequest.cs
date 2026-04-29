namespace ERP.Api.Contracts.MasterData;

public sealed record CreateCustomerRequest(
    long CompanyId,
    string Name,
    string? TaxId);
