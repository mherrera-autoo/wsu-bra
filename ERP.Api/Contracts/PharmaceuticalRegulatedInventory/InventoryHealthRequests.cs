using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;

namespace ERP.Api.Contracts.PharmaceuticalRegulatedInventory;

public sealed record PlaceQuarantineRequest(
    long CompanyId,
    long StockBatchId,
    string Reason,
    StockBatchHealthStatus Status);

public sealed record ReleaseQuarantineRequest(
    long CompanyId,
    long StockBatchId);

public sealed record CreateRecallRequest(
    long CompanyId,
    long StockBatchId,
    string Reason,
    decimal? Quantity);

public sealed record CreateWasteRequest(
    long CompanyId,
    long StockBatchId,
    decimal Quantity,
    string Reason);
