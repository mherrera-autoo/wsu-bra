using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;

namespace ERP.Api.Contracts.PharmaceuticalRegulatedInventory;

public sealed record ControlledSubstanceEntryRequest(
    long CompanyId,
    DateTime Date,
    long ProductId,
    string BatchNumber,
    decimal Quantity,
    string ReferenceDocument,
    string IspFolio,
    string IspReason,
    string IspOriginDestination,
    string IspSupplierOrPatient,
    string IspPrescriber,
    string IspTaxReference);

public sealed record ControlledSubstanceExitRequest(
    long CompanyId,
    DateTime Date,
    long ProductId,
    string BatchNumber,
    decimal Quantity,
    string ReferenceDocument,
    string IspFolio,
    string IspReason,
    string IspOriginDestination,
    string IspSupplierOrPatient,
    string IspPrescriber,
    string IspTaxReference);

public sealed record ControlledSubstanceAdjustmentRequest(
    long CompanyId,
    DateTime Date,
    long ProductId,
    string BatchNumber,
    decimal Quantity,
    bool IsIncrease,
    string ReferenceDocument,
    string IspFolio,
    string IspReason,
    string IspOriginDestination,
    string IspSupplierOrPatient,
    string IspPrescriber,
    string IspTaxReference);

public sealed record ControlledSubstanceLedgerQuery(
    DateTime? From,
    DateTime? To,
    long? ProductId,
    ControlledMovementType? MovementType,
    string? BatchNumber,
    string? IspFolio);

public sealed record ControlledSubstanceLedgerExportQuery(
    DateTime? From,
    DateTime? To,
    long? ProductId,
    ControlledMovementType? MovementType,
    string? BatchNumber,
    string? IspFolio,
    ControlledSubstanceExportFormat Format = ControlledSubstanceExportFormat.Json);

public enum ControlledSubstanceExportFormat
{
    Json,
    Csv
}
