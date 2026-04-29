using System;

namespace ERP.Api.Contracts.PharmaceuticalRegulatedInventory;

public sealed record RegisterReturnRequest(
    long CompanyId,
    long ProductId,
    long WarehouseId,
    decimal Quantity,
    string BatchNumber,
    DateTime? ExpiryDate,
    string? Reason,
    long? CustomerId,
    string? PatientReference,
    string? ReferenceId);

public sealed record PharmaceuticalRegulatedInventoryTraceEventResponse(
    long Id,
    long StockBatchId,
    long ProductId,
    long WarehouseId,
    string BatchNumber,
    DateTime? ExpiryDate,
    decimal Quantity,
    string? MovementType,
    string ReferenceType,
    string ReferenceId,
    long? SupplierId,
    long? CustomerId,
    string? PatientReference,
    string? Reason,
    DateTime CreatedAt);
