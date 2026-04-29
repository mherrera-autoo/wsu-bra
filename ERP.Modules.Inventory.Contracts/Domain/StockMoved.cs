namespace ERP.Modules.Inventory.Domain;

public sealed record StockMoved(
    InventoryMovement Movement,
    IReadOnlyList<StockMovementBatchInput> BatchInputs,
    StockMovementReference? Reference);

public sealed record StockMovementBatchInput(
    string BatchNumber,
    DateTime? ExpiryDate,
    decimal Quantity);

public sealed record StockMovementReference(
    long? SupplierId,
    long? CustomerId,
    string? PatientReference);
