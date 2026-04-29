## WSU Sprint 1 Design Notes

- Existing attributes found in MasterData:
  - `Product`: `Sku`, `Name`, `PublicId`.
  - `Warehouse`: `Code`, `Name`, `PublicId`.
- Added to `Product` for WSU interoperability:
  - `IsStackable`, `LengthCm`, `WidthCm`, `WeightKg`, `StorageType`.
- Added to `Warehouse` for WSU interoperability:
  - `Location`, `Position`.
- `wsu.ClientProductWarehouses` was legacy and has been fully removed.
- Persisted only as transaction snapshot in `wsu.OrderItems` (for imported-line traceability):
  - Supplier/customer fields, operator fields, movement start/end times.
  - Line position values.
  - Purchase/sales lot and dimension fields.
  - Imported unit prices (`PrecioCompraUnitario`, `PrecioVentaUnitario`).
  - Order line denormalized identifiers (`SkuWsu`, `NombreSkuWsu`, etc.).
- Sprint 1 movement derivation:
  - `Inbound`/`Replenishment` => `Inbound` with positive `SignedQuantity`.
  - `Consumption`/`Adjustment` => `Outbound` with negative `SignedQuantity`.
  - `Quantity = abs(VariacionStock)`.

## FIFO Inventory Valuation (Minimal Extension)

- Reused entities:
  - `wsu.Orders` and `wsu.OrderItems` remain the stock source of truth.
  - No `InventoryLots` table is introduced. Inbound `OrderItems` are logical lots.
  - No `WarehousePositions` table is introduced (position remains a column/snapshot).
- New persistence for traceability:
  - `wsu.OrderItemConsumptions` records outbound-to-inbound FIFO links.
  - Fields: `OutOrderItemId`, `InOrderItemId`, `Quantity`, `UnitCost`, `TotalCost`.
- `OrderItems` extension:
  - Added `QuantityAvailable` to track remaining inbound quantity.
  - Backfill strategy in migration:
    - `Inbound`/`Replenishment` => `QuantityAvailable = Quantity`.
    - `Consumption`/`Adjustment` => `QuantityAvailable = 0`.
- FIFO algorithm:
  - Applied when creating outbound orders (`Consumption`, `Adjustment`).
  - Candidate inbound items are filtered by same `CompanyPublicId`, same warehouse scope, same `ProductPublicId`, and `QuantityAvailable > 0`.
  - Ordered by `Orders.MovementDate ASC`, then `OrderItems.Id ASC`.
  - Multiple `OrderItemConsumptions` rows are created if one outbound line spans multiple inbound lots.
  - Inbound `QuantityAvailable` is reduced per consumption row.
  - If stock is insufficient, operation fails and transaction is rolled back.
- Costing rules:
  - FIFO cost source is inbound lot cost (`OrderItem.UnitPrice`, fallback to `PrecioCompraUnitario`).
  - Product master/list prices are not used for FIFO valuation.
- Operational read models exposed:
  - Available stock by product and warehouse.
  - Inventory valuation from remaining inbound stock.
  - FIFO detail for outbound orders (full audit trace).
