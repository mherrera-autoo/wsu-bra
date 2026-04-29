using System;
using System.Collections.Generic;

namespace ERP.Modules.Sales.Contracts;

public sealed record SalesShipmentRequested(
    long CompanyId,
    long DocumentId,
    long CustomerId,
    long WarehouseId,
    IReadOnlyList<SalesShipmentLine> Lines);

public sealed record SalesShipmentLine(
    long ProductId,
    decimal Quantity,
    IReadOnlyList<SalesShipmentBatchInput>? BatchInputs);

public sealed record SalesShipmentBatchInput(
    string BatchNumber,
    DateTime? ExpiryDate,
    decimal Quantity);
