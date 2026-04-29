using ERP.Modules.Wsu.Application.Repositories;
using ERP.Modules.Wsu.Domain;
using ERP.Modules.Rfid.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly ErpDbContext _dbContext;

    public OrderRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(Order item, CancellationToken cancellationToken = default)
        => _dbContext.WsuOrders.AddAsync(item, cancellationToken).AsTask();

    public Task<Order?> GetByPublicIdAsync(Guid companyPublicId, Guid publicId, CancellationToken cancellationToken = default)
        => _dbContext.WsuOrders
            .Include(item => item.Items)
            .FirstOrDefaultAsync(item => item.CompanyPublicId == companyPublicId && item.PublicId == publicId, cancellationToken);

    public async Task<IReadOnlyList<OrderListItem>> ListAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        OrderType? orderType,
        DateTimeOffset? movementDateFrom,
        DateTimeOffset? movementDateTo,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(companyPublicId, warehousePublicId, orderType, movementDateFrom, movementDateTo);

        var pagedOrders = await query
            .AsNoTracking()
            .OrderByDescending(item => item.MovementDate)
            .ThenByDescending(item => item.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        if (pagedOrders.Count == 0)
        {
            return Array.Empty<OrderListItem>();
        }

        var orderIds = pagedOrders.Select(item => item.Id).ToArray();
        var orderStats = await _dbContext.WsuOrderItems
            .AsNoTracking()
            .Where(item => orderIds.Contains(item.OrderId))
            .GroupBy(item => item.OrderId)
            .Select(group => new
            {
                OrderId = group.Key,
                Count = group.Count(),
                TagsACargar = group.Sum(item => Math.Abs(item.VariacionStock))
            })
            .ToDictionaryAsync(item => item.OrderId, cancellationToken);

        var warehousePublicIds = pagedOrders
            .Where(item => item.WarehousePublicId.HasValue)
            .Select(item => item.WarehousePublicId!.Value)
            .Distinct()
            .ToArray();

        var warehousesByPublicId = await _dbContext.Warehouses
            .AsNoTracking()
            .Where(item => warehousePublicIds.Contains(item.PublicId))
            .Select(item => new { item.PublicId, item.Code, item.Name, item.Location })
            .ToDictionaryAsync(item => item.PublicId, cancellationToken);

        return pagedOrders
            .Select(order =>
            {
                string? warehouseName = null;
                string? warehouseLocation = null;
                string? warehouseCode = null;
                if (order.WarehousePublicId.HasValue
                    && warehousesByPublicId.TryGetValue(order.WarehousePublicId.Value, out var warehouse))
                {
                    warehouseCode = warehouse.Code;
                    warehouseName = warehouse.Name;
                    warehouseLocation = warehouse.Location;
                }

                var itemsCount = 0;
                var tagsACargar = 0m;
                if (orderStats.TryGetValue(order.Id, out var stats))
                {
                    itemsCount = stats.Count;
                    tagsACargar = stats.TagsACargar;
                }

                return new OrderListItem(
                    order.Id,
                    order.PublicId,
                    order.OrderType,
                    order.OrderNumber,
                    order.ExternalOrderNumber,
                    order.Status,
                    order.MovementDate,
                    order.CreatedByUserPublicId,
                    order.SourceType,
                    order.Notes,
                    order.IsEpcLoadConfirmed,
                    order.IsMovementProgrammed,
                    order.IsEpcSkuMatchCompleted,
                    itemsCount,
                    tagsACargar,
                    order.CreatedAt,
                    order.UpdatedAt,
                    warehouseCode,
                    warehouseName,
                    warehouseLocation);
            })
            .ToList();
    }

    public Task<int> CountAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        OrderType? orderType,
        DateTimeOffset? movementDateFrom,
        DateTimeOffset? movementDateTo,
        CancellationToken cancellationToken = default)
        => ApplyFilter(companyPublicId, warehousePublicId, orderType, movementDateFrom, movementDateTo)
            .CountAsync(cancellationToken);

    private IQueryable<Order> ApplyFilter(
        Guid companyPublicId,
        Guid? warehousePublicId,
        OrderType? orderType,
        DateTimeOffset? movementDateFrom,
        DateTimeOffset? movementDateTo)
    {
        var query = _dbContext.WsuOrders.Where(item => item.CompanyPublicId == companyPublicId);

        if (warehousePublicId.HasValue)
        {
            query = query.Where(item => item.WarehousePublicId == warehousePublicId.Value);
        }

        if (orderType.HasValue)
        {
            query = query.Where(item => item.OrderType == orderType.Value);
        }

        if (movementDateFrom.HasValue)
        {
            query = query.Where(item => item.MovementDate >= movementDateFrom.Value);
        }

        if (movementDateTo.HasValue)
        {
            query = query.Where(item => item.MovementDate <= movementDateTo.Value);
        }

        return query;
    }
}

public sealed class OrderItemRepository : IOrderItemRepository
{
    private readonly ErpDbContext _dbContext;

    public OrderItemRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(OrderItem item, CancellationToken cancellationToken = default)
        => _dbContext.WsuOrderItems.AddAsync(item, cancellationToken).AsTask();

    public async Task<IReadOnlyList<OrderItem>> ListAvailableInboundForFifoAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        Guid productPublicId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.WsuOrderItems
            .Include(item => item.Order)
            .Where(item => item.Order.CompanyPublicId == companyPublicId
                && item.ProductPublicId == productPublicId
                && item.QuantityAvailableForFifo > 0m
                && (item.Order.OrderType == OrderType.Inbound || item.Order.OrderType == OrderType.Replenishment)
                && item.Order.Status == OrderStatus.Confirmed);

        if (warehousePublicId.HasValue)
        {
            query = query.Where(item => item.Order.WarehousePublicId == warehousePublicId.Value);
        }

        return await query
            .OrderBy(item => item.Order.MovementDate)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<decimal> GetAvailableStockAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        Guid productPublicId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.WsuOrderItems
            .Where(item => item.Order.CompanyPublicId == companyPublicId
                && item.ProductPublicId == productPublicId
                && (item.Order.OrderType == OrderType.Inbound || item.Order.OrderType == OrderType.Replenishment)
                && item.Order.Status == OrderStatus.Confirmed);

        if (warehousePublicId.HasValue)
        {
            query = query.Where(item => item.Order.WarehousePublicId == warehousePublicId.Value);
        }

        return query.SumAsync(item => item.QuantityAvailableForFifo, cancellationToken);
    }

    public Task<decimal> GetInventoryValuationAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.WsuOrderItems
            .Where(item => item.Order.CompanyPublicId == companyPublicId
                && item.QuantityAvailableForFifo > 0m
                && (item.Order.OrderType == OrderType.Inbound || item.Order.OrderType == OrderType.Replenishment)
                && item.Order.Status == OrderStatus.Confirmed);

        if (warehousePublicId.HasValue)
        {
            query = query.Where(item => item.Order.WarehousePublicId == warehousePublicId.Value);
        }

        return query.SumAsync(item => item.QuantityAvailableForFifo * (item.PrecioCompraUnitario ?? 0m), cancellationToken);
    }
}

public sealed class OrderItemConsumptionRepository : IOrderItemConsumptionRepository
{
    private readonly ErpDbContext _dbContext;

    public OrderItemConsumptionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(OrderItemConsumption item, CancellationToken cancellationToken = default)
        => _dbContext.WsuOrderItemConsumptions.AddAsync(item, cancellationToken).AsTask();

    public async Task<IReadOnlyList<OrderItemConsumption>> ListByOutOrderItemIdAsync(long outOrderItemId, CancellationToken cancellationToken = default)
        => await _dbContext.WsuOrderItemConsumptions
            .Include(item => item.InOrderItem)
            .ThenInclude(item => item.Order)
            .Where(item => item.OutOrderItemId == outOrderItemId)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);

    public Task<decimal> GetTotalCostByOutOrderItemIdAsync(long outOrderItemId, CancellationToken cancellationToken = default)
        => _dbContext.WsuOrderItemConsumptions
            .Where(item => item.OutOrderItemId == outOrderItemId)
            .SumAsync(item => item.TotalCost, cancellationToken);
}

public sealed class OrderItemReconciliationRepository : IOrderItemReconciliationRepository
{
    private readonly ErpDbContext _dbContext;

    public OrderItemReconciliationRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(OrderItemReconciliation item, CancellationToken cancellationToken = default)
        => _dbContext.WsuOrderItemReconciliations.AddAsync(item, cancellationToken).AsTask();
}

public sealed class OrderItemEpcAssignmentRepository : IOrderItemEpcAssignmentRepository
{
    private readonly ErpDbContext _dbContext;

    public OrderItemEpcAssignmentRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> AnyByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
        => _dbContext.WsuOrderItemEpcAssignments.AnyAsync(item => item.OrderId == orderId, cancellationToken);

    public async Task<IReadOnlyList<string>> ListEpcsByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
        => await _dbContext.WsuOrderItemEpcAssignments
            .AsNoTracking()
            .Where(item => item.OrderId == orderId)
            .Select(item => item.Epc)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task DeleteByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.WsuOrderItemEpcAssignments
            .Where(item => item.OrderId == orderId)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return;
        }

        _dbContext.WsuOrderItemEpcAssignments.RemoveRange(items);
    }

    public async Task AddRangeAsync(IReadOnlyCollection<OrderItemEpcAssignment> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return;
        }

        await _dbContext.WsuOrderItemEpcAssignments.AddRangeAsync(items, cancellationToken);
    }

    public async Task<OrderItemEpcCheckUpdateResult> ApplyChecksAsync(long orderId, IReadOnlyCollection<OrderItemEpcCheckUpdateItem> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            var emptyTotal = await _dbContext.WsuOrderItemEpcAssignments
                .AsNoTracking()
                .CountAsync(item => item.OrderId == orderId, cancellationToken);
            var emptyChecked = await _dbContext.WsuOrderItemEpcAssignments
                .AsNoTracking()
                .CountAsync(item => item.OrderId == orderId && item.IsChecked, cancellationToken);
            return new OrderItemEpcCheckUpdateResult(0, emptyTotal, emptyChecked, []);
        }

        var normalizedUpdates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            var epc = item.Epc?.Trim();
            if (string.IsNullOrWhiteSpace(epc))
            {
                continue;
            }

            normalizedUpdates[epc] = item.IsChecked;
        }

        if (normalizedUpdates.Count == 0)
        {
            var totalWithoutUpdates = await _dbContext.WsuOrderItemEpcAssignments
                .AsNoTracking()
                .CountAsync(item => item.OrderId == orderId, cancellationToken);
            var checkedWithoutUpdates = await _dbContext.WsuOrderItemEpcAssignments
                .AsNoTracking()
                .CountAsync(item => item.OrderId == orderId && item.IsChecked, cancellationToken);
            return new OrderItemEpcCheckUpdateResult(0, totalWithoutUpdates, checkedWithoutUpdates, []);
        }

        var assignments = await _dbContext.WsuOrderItemEpcAssignments
            .Where(item => item.OrderId == orderId)
            .ToListAsync(cancellationToken);

        var assignmentsByEpc = assignments
            .GroupBy(item => item.Epc.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var notFoundEpcs = new List<string>();
        var updatedCount = 0;

        foreach (var update in normalizedUpdates)
        {
            if (!assignmentsByEpc.TryGetValue(update.Key, out var assignmentItems))
            {
                notFoundEpcs.Add(update.Key);
                continue;
            }

            foreach (var assignment in assignmentItems)
            {
                if (assignment.IsChecked == update.Value)
                {
                    continue;
                }

                assignment.SetChecked(update.Value);
                updatedCount++;
            }
        }

        var total = assignments.Count;
        var checkedCount = assignments.Count(item => item.IsChecked);

        return new OrderItemEpcCheckUpdateResult(
            updatedCount,
            total,
            checkedCount,
            notFoundEpcs
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }

    public async Task<IReadOnlyList<OrderItemEpcAssignmentListItem>> ListDetailedByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
        => await (from assignment in _dbContext.WsuOrderItemEpcAssignments
                  join orderItem in _dbContext.WsuOrderItems on assignment.OrderItemId equals orderItem.Id
                  where assignment.OrderId == orderId
                  orderby assignment.OrderItemId, assignment.Epc
                  select new OrderItemEpcAssignmentListItem(
                      orderItem.SkuWsu,
                      orderItem.NombreSkuWsu,
                      orderItem.SkuProveedor,
                      orderItem.NombreSkuProveedor,
                      orderItem.SkuCliente,
                      orderItem.NombreSkuCliente,
                      assignment.Epc,
                      assignment.IsChecked))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
}

public sealed class WsuRfidTagLookupRepository : IWsuRfidTagLookupRepository
{
    private readonly ErpDbContext _dbContext;

    public WsuRfidTagLookupRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlySet<string>> ListExistingEpcsAsync(Guid companyPublicId, Guid warehousePublicId, IReadOnlyCollection<string> epcs, CancellationToken cancellationToken = default)
    {
        if (epcs.Count == 0)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        var existing = await _dbContext.RfidTags
            .AsNoTracking()
            .Where(item => item.CompanyPublicId == companyPublicId
                && item.WarehousePublicId == warehousePublicId
                && epcs.Contains(item.Epc))
            .Select(item => item.Epc)
            .ToListAsync(cancellationToken);

        return new HashSet<string>(existing, StringComparer.OrdinalIgnoreCase);
    }

    public async Task AddMissingAsync(Guid companyPublicId, Guid warehousePublicId, IReadOnlyCollection<string> epcs, CancellationToken cancellationToken = default)
    {
        if (epcs.Count == 0)
        {
            return;
        }

        var normalizedEpcs = epcs
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (normalizedEpcs.Length == 0)
        {
            return;
        }

        var existing = await ListExistingEpcsAsync(companyPublicId, warehousePublicId, normalizedEpcs, cancellationToken);

        var newTags = normalizedEpcs
            .Where(item => !existing.Contains(item))
            .Select(item => RfidTag.Create(companyPublicId, warehousePublicId, item, "Created from WSU EPC import."))
            .ToArray();

        if (newTags.Length == 0)
        {
            return;
        }

        await _dbContext.RfidTags.AddRangeAsync(newTags, cancellationToken);
    }

    public async Task DeleteByCompanyWarehouseAndEpcsAsync(Guid companyPublicId, Guid warehousePublicId, IReadOnlyCollection<string> epcs, CancellationToken cancellationToken = default)
    {
        if (epcs.Count == 0)
        {
            return;
        }

        var normalizedEpcs = epcs
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (normalizedEpcs.Length == 0)
        {
            return;
        }

        var tags = await _dbContext.RfidTags
            .Where(item => item.CompanyPublicId == companyPublicId
                && item.WarehousePublicId == warehousePublicId
                && normalizedEpcs.Contains(item.Epc))
            .ToListAsync(cancellationToken);

        if (tags.Count == 0)
        {
            return;
        }

        _dbContext.RfidTags.RemoveRange(tags);
    }
}

public sealed class OperatorEnrollmentValidator : IOperatorEnrollmentValidator
{
    private readonly ErpDbContext _dbContext;

    public OperatorEnrollmentValidator(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> IsEnrolledAsync(Guid companyPublicId, string operatorCode, CancellationToken cancellationToken = default)
    {
        var normalizedCode = operatorCode.Trim();

        return _dbContext.Operators
            .Where(item => item.CompanyPublicId == companyPublicId && item.IsActive && item.Code == normalizedCode)
            .AnyAsync(operatorItem => _dbContext.OperatorCredentials
                .Any(credential => credential.OperatorId == operatorItem.Id
                    && credential.CompanyPublicId == companyPublicId
                    && credential.IsActive
                    && credential.FaceTemplateId != null
                    && credential.FaceTemplateId != ""
                    && credential.NfcCardUid != null
                    && credential.NfcCardUid != ""), cancellationToken);
    }
}

public sealed class WsuOrderMovementOperatorRepository : IWsuOrderMovementOperatorRepository
{
    private readonly ErpDbContext _dbContext;

    public WsuOrderMovementOperatorRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<string>> ListOperatorCodesByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
        => await _dbContext.WsuOrderMovementOperators
            .AsNoTracking()
            .Where(item => item.OrderId == orderId && item.CodigoOperador != null && item.CodigoOperador != "")
            .Select(item => item.CodigoOperador)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task DeleteByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
    {
        var items = await _dbContext.WsuOrderMovementOperators
            .Where(item => item.OrderId == orderId)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return;
        }

        _dbContext.WsuOrderMovementOperators.RemoveRange(items);
    }

    public async Task AddRangeAsync(IReadOnlyCollection<WsuOrderMovementOperator> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return;
        }

        await _dbContext.WsuOrderMovementOperators.AddRangeAsync(items, cancellationToken);
    }
}

public sealed class WsuInventoryMovementRepository : IWsuInventoryMovementRepository
{
    private readonly ErpDbContext _dbContext;

    public WsuInventoryMovementRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddAsync(WsuInventoryMovement item, CancellationToken cancellationToken = default)
        => _dbContext.WsuInventoryMovements.AddAsync(item, cancellationToken).AsTask();
}

public sealed class WsuInventoryMovementOperatorRepository : IWsuInventoryMovementOperatorRepository
{
    private readonly ErpDbContext _dbContext;

    public WsuInventoryMovementOperatorRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<long>> ListMovementIdsByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
        => await _dbContext.WsuInventoryMovements
            .AsNoTracking()
            .Where(item => item.OrderId == orderId)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> ListOperatorCodesByOrderIdAsync(long orderId, CancellationToken cancellationToken = default)
        => await _dbContext.WsuInventoryMovementOperators
            .AsNoTracking()
            .Where(item => item.InventoryMovement.OrderId == orderId && item.CodigoOperador != null && item.CodigoOperador != "")
            .Select(item => item.CodigoOperador)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task DeleteByMovementIdsAsync(IReadOnlyCollection<long> movementIds, CancellationToken cancellationToken = default)
    {
        if (movementIds.Count == 0)
        {
            return;
        }

        var items = await _dbContext.WsuInventoryMovementOperators
            .Where(item => movementIds.Contains(item.InventoryMovementId))
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return;
        }

        _dbContext.WsuInventoryMovementOperators.RemoveRange(items);
    }

    public async Task AddRangeAsync(IReadOnlyCollection<WsuInventoryMovementOperator> items, CancellationToken cancellationToken = default)
    {
        if (items.Count == 0)
        {
            return;
        }

        await _dbContext.WsuInventoryMovementOperators.AddRangeAsync(items, cancellationToken);
    }
}
