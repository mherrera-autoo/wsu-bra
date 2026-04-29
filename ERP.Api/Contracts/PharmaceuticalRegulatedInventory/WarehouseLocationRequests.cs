namespace ERP.Api.Contracts.PharmaceuticalRegulatedInventory;

public sealed record CreateWarehouseLocationRequest(
    long CompanyId,
    long WarehouseId,
    string Aisle,
    string Rack,
    string Side,
    int Level,
    int Slot,
    decimal Capacity,
    bool IsPalletSlot,
    bool IsStackable,
    string? Notes);

public sealed record UpdateWarehouseLocationRequest(
    long CompanyId,
    string Aisle,
    string Rack,
    string Side,
    int Level,
    int Slot,
    decimal Capacity,
    bool IsPalletSlot,
    bool IsStackable,
    string? Notes);
