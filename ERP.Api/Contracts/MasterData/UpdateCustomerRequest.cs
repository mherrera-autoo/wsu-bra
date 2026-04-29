namespace ERP.Api.Contracts.MasterData;

public sealed record UpdateCustomerRequest(
    long CompanyId,
    string Name,
    string? TaxId);
