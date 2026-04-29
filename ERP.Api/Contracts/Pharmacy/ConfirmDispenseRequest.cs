using System;

namespace ERP.Api.Contracts.Pharmacy;

// TODO: Move dispensing DTOs to ERP.Api.Contracts.RetailPharmacy (ERP.Modules.RetailPharmacy).
public sealed record ConfirmDispenseRequest(
    long CompanyId,
    long WarehouseId,
    IReadOnlyList<DispenseBatchInput>? BatchInputs);

public sealed record DispenseBatchInput(
    long ProductId,
    string BatchNumber,
    DateTime? ExpiryDate,
    decimal Quantity);
