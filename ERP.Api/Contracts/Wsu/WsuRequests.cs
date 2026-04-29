using ERP.Modules.Wsu.Domain;
using Microsoft.AspNetCore.Http;

namespace ERP.Api.Contracts.Wsu;

public sealed record CreateWsuOrderRequest(
    Guid? CompanyPublicId,
    Guid? WarehousePublicId,
    OrderType OrderType,
    string OrderNumber,
    string? ExternalOrderNumber,
    OrderStatus Status,
    DateTimeOffset? MovementDate,
    Guid? CreatedByUserPublicId,
    SourceType SourceType,
    string? Notes,
    IReadOnlyList<CreateWsuOrderItemRequest> Items);

public sealed record CreateWsuOrderItemRequest(
    Guid? ProductPublicId,
    string? SkuWsu,
    string? NombreSkuWsu,
    string? SkuProveedor,
    string? NombreSkuProveedor,
    string? RutProveedor,
    string? RsProveedor,
    string? SkuCliente,
    string? NombreSkuCliente,
    string? RutCliente,
    string? RsCliente,
    string? Posicion,
    string? UnidadDeMedida,
    decimal? LoteMinimoCompra,
    decimal? LargoCompraCm,
    decimal? AnchoCompraCm,
    decimal? AltoCompraCm,
    decimal? PesoCompraKg,
    bool? ApilableCompra,
    string? TipoAlmacenamientoCompra,
    decimal? LoteMinimoVenta,
    decimal? LargoVentaCm,
    decimal? AnchoVentaCm,
    decimal? AltoVentaCm,
    decimal? PesoVentaKg,
    bool? ApilableVenta,
    string? TipoAlmacenamientoVenta,
    decimal? PrecioCompraUnitario,
    decimal? PrecioVentaUnitario,
    decimal VariacionStock,
    decimal? StockMinimo,
    string? Notes);

public sealed record WsuOrderResponse(
    long Id,
    Guid PublicId,
    Guid CompanyPublicId,
    Guid? WarehousePublicId,
    OrderType OrderType,
    string OrderNumber,
    string? ExternalOrderNumber,
    OrderStatus Status,
    DateTimeOffset MovementDate,
    Guid? CreatedByUserPublicId,
    SourceType SourceType,
    string? Notes,
    bool IsEpcLoadConfirmed,
    int ItemsCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record WsuOrderDetailItemResponse(
    long Id,
    Guid PublicId,
    Guid? ProductPublicId,
    string? SkuWsu,
    string? NombreSkuWsu,
    string? SkuProveedor,
    string? NombreSkuProveedor,
    string? RutProveedor,
    string? RsProveedor,
    string? SkuCliente,
    string? NombreSkuCliente,
    string? RutCliente,
    string? RsCliente,
    string? Posicion,
    string? UnidadDeMedida,
    decimal? LoteMinimoCompra,
    decimal? LargoCompraCm,
    decimal? AnchoCompraCm,
    decimal? AltoCompraCm,
    decimal? PesoCompraKg,
    bool? ApilableCompra,
    string? TipoAlmacenamientoCompra,
    decimal? LoteMinimoVenta,
    decimal? LargoVentaCm,
    decimal? AnchoVentaCm,
    decimal? AltoVentaCm,
    decimal? PesoVentaKg,
    bool? ApilableVenta,
    string? TipoAlmacenamientoVenta,
    decimal? PrecioCompraUnitario,
    decimal? PrecioVentaUnitario,
    decimal VariacionStock,
    decimal? StockMinimo,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record WsuOrderDetailResponse(
    long Id,
    Guid PublicId,
    Guid CompanyPublicId,
    Guid? WarehousePublicId,
    OrderType OrderType,
    string OrderNumber,
    string? ExternalOrderNumber,
    OrderStatus Status,
    DateTimeOffset MovementDate,
    Guid? CreatedByUserPublicId,
    SourceType SourceType,
    string? Notes,
    bool IsEpcLoadConfirmed,
    bool IsMovementProgrammed,
    bool IsEpcSkuMatchCompleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<WsuOrderDetailItemResponse> Items);

public sealed record WsuOrderListItemResponse(
    long Id,
    Guid PublicId,
    OrderType OrderType,
    string OrderNumber,
    string? ExternalOrderNumber,
    OrderStatus Status,
    DateTimeOffset MovementDate,
    Guid? CreatedByUserPublicId,
    SourceType SourceType,
    string? Notes,
    bool IsEpcLoadConfirmed,
    bool IsMovementProgrammed,
    bool IsEpcSkuMatchCompleted,
    int ItemsCount,
    decimal TagsACargar,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? WarehouseCode,
    string? WarehouseName,
    string? WarehouseLocation);

public sealed record UpdateWsuOrderEpcChecksRequest(
    Guid? CompanyPublicId,
    IReadOnlyList<UpdateWsuOrderEpcCheckItemRequest> Items);

public sealed record UpdateWsuOrderEpcCheckItemRequest(
    string Epc,
    bool IsChecked);

public sealed record UpdateWsuOrderEpcChecksResponse(
    Guid OrderPublicId,
    int UpdatedCount,
    int TotalAssignments,
    int CheckedAssignments,
    bool IsEpcSkuMatchCompleted,
    IReadOnlyList<string> NotFoundEpcs);

public sealed record WsuOrderEpcAssignmentListItemResponse(
    string? SkuWsu,
    string? NombreSkuWsu,
    string? SkuProveedor,
    string? NombreSkuProveedor,
    string? SkuCliente,
    string? NombreSkuCliente,
    string Epc,
    bool IsChecked);

public sealed class ImportWsuOrderExcelRequest
{
    public required IFormFile File { get; init; }
    public Guid? CompanyPublicId { get; init; }
    public Guid? WarehousePublicId { get; init; }
    public OrderType OrderType { get; init; }
    public OrderStatus? Status { get; init; }
    public DateTimeOffset? MovementDate { get; init; }
    public SourceType? SourceType { get; init; }
    public string? ExternalOrderNumber { get; init; }
    public string? Notes { get; init; }
}

public sealed class ImportWsuOrderEpcExcelRequest
{
    public required IFormFile File { get; init; }
    public Guid? CompanyPublicId { get; init; }
}

public sealed record WsuOrderEpcImportResponse(
    Guid OrderPublicId,
    int ExpectedTags,
    int ImportedTags);

public sealed record ReconcileWsuOrderRfidRequest(
    Guid? CompanyPublicId,
    DateTimeOffset? ReconciledAt,
    Guid? ReconciledByUserPublicId,
    string? Notes,
    IReadOnlyList<ReconcileWsuOrderRfidItemRequest> Items);

public sealed record ReconcileWsuOrderRfidItemRequest(
    Guid OrderItemPublicId,
    decimal ReconciledQuantity);

public sealed record ChangeWsuOrderStatusRequest(
    Guid? CompanyPublicId,
    Guid? UserPublicId,
    string? Notes);

public sealed record AssignWsuOrderMovementOperatorItemRequest(
    string CodigoOperador,
    string? NombreOperador,
    DateOnly? FechaMovimiento,
    TimeOnly? HoraInicioMovimiento,
    TimeOnly? HoraFinMovimiento);

public sealed record AssignWsuOrderMovementOperatorsRequest(
    Guid? CompanyPublicId,
    IReadOnlyList<AssignWsuOrderMovementOperatorItemRequest> Items);

public sealed record WsuStockAvailabilityResponse(
    Guid CompanyPublicId,
    Guid? WarehousePublicId,
    Guid ProductPublicId,
    decimal QuantityAvailableForFifo);

public sealed record WsuInventoryValuationResponse(
    Guid CompanyPublicId,
    Guid? WarehousePublicId,
    decimal InventoryValuation);

public sealed record WsuFifoConsumptionLineResponse(
    long ConsumptionId,
    Guid InOrderItemPublicId,
    string InOrderNumber,
    DateTimeOffset InMovementDate,
    decimal Quantity,
    decimal UnitCost,
    decimal TotalCost);

public sealed record WsuFifoOutItemDetailResponse(
    Guid OutOrderItemPublicId,
    Guid? ProductPublicId,
    decimal RequestedQuantity,
    decimal AppliedTotalCost,
    IReadOnlyList<WsuFifoConsumptionLineResponse> Consumptions);

public sealed record WsuOrderFifoDetailResponse(
    Guid OrderPublicId,
    string OrderNumber,
    DateTimeOffset MovementDate,
    decimal TotalAppliedCost,
    IReadOnlyList<WsuFifoOutItemDetailResponse> Items);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Skip, int Take);
