using ERP.Modules.Wsu.Domain;

namespace ERP.Modules.Wsu.Application.Repositories;

public sealed record OrderListItem(
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
