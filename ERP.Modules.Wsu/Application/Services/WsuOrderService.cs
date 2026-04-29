using ERP.Modules.Wsu.Application.Repositories;
using ERP.Modules.Wsu.Domain;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Wsu.Application.Services;

public sealed record CreateWsuOrderItemInput(
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

public sealed record ReconcileWsuOrderItemInput(
    Guid OrderItemPublicId,
    decimal ReconciledQuantity);

public sealed record ReconcileWsuOrderInput(
    Guid CompanyPublicId,
    Guid OrderPublicId,
    DateTimeOffset? ReconciledAt,
    Guid? ReconciledByUserPublicId,
    string? Notes,
    IReadOnlyList<ReconcileWsuOrderItemInput> Items);

public sealed record ChangeWsuOrderStatusInput(
    Guid CompanyPublicId,
    Guid OrderPublicId,
    Guid? UserPublicId,
    string? Notes);

public sealed record AssignWsuOrderMovementOperatorInput(
    string CodigoOperador,
    string? NombreOperador,
    DateOnly? FechaMovimiento,
    TimeOnly? HoraInicioMovimiento,
    TimeOnly? HoraFinMovimiento);

public sealed record AssignWsuOrderMovementOperatorsInput(
    Guid CompanyPublicId,
    Guid OrderPublicId,
    IReadOnlyList<AssignWsuOrderMovementOperatorInput> Items);

public sealed record CreateWsuOrderInput(
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
    IReadOnlyList<CreateWsuOrderItemInput> Items);

public sealed record ImportWsuOrderEpcLineInput(
    int RowNumber,
    string Epc);

public sealed record ImportWsuOrderEpcsInput(
    Guid CompanyPublicId,
    Guid OrderPublicId,
    IReadOnlyList<ImportWsuOrderEpcLineInput> Items);

public sealed record ImportWsuOrderEpcsResult(
    int ExpectedTags,
    int ImportedTags);

public sealed class WsuOrderService
{
    private const int MaxDuplicateEpcsToShow = 20;

    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IOrderItemConsumptionRepository _orderItemConsumptionRepository;
    private readonly IOrderItemReconciliationRepository _orderItemReconciliationRepository;
    private readonly IOrderItemEpcAssignmentRepository _orderItemEpcAssignmentRepository;
    private readonly IWsuOrderMovementOperatorRepository _orderMovementOperatorRepository;
    private readonly IWsuRfidTagLookupRepository _rfidTagLookupRepository;
    private readonly IOperatorEnrollmentValidator _operatorEnrollmentValidator;
    private readonly IProductRepository _productRepository;
    private readonly ICompanyUnitOfMeasureRepository _companyUnitOfMeasureRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IWsuInventoryMovementRepository _movementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly bool _validarOperadorEnOrden;

    public WsuOrderService(
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IOrderItemConsumptionRepository orderItemConsumptionRepository,
        IOrderItemReconciliationRepository orderItemReconciliationRepository,
        IOrderItemEpcAssignmentRepository orderItemEpcAssignmentRepository,
        IWsuOrderMovementOperatorRepository orderMovementOperatorRepository,
        IWsuRfidTagLookupRepository rfidTagLookupRepository,
        IOperatorEnrollmentValidator operatorEnrollmentValidator,
        IProductRepository productRepository,
        ICompanyUnitOfMeasureRepository companyUnitOfMeasureRepository,
        ICompanyRepository companyRepository,
        IWsuInventoryMovementRepository movementRepository,
        IUnitOfWork unitOfWork,
        bool validarOperadorEnOrden = false)
    {
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _orderItemConsumptionRepository = orderItemConsumptionRepository;
        _orderItemReconciliationRepository = orderItemReconciliationRepository;
        _orderItemEpcAssignmentRepository = orderItemEpcAssignmentRepository;
        _orderMovementOperatorRepository = orderMovementOperatorRepository;
        _rfidTagLookupRepository = rfidTagLookupRepository;
        _operatorEnrollmentValidator = operatorEnrollmentValidator;
        _productRepository = productRepository;
        _companyUnitOfMeasureRepository = companyUnitOfMeasureRepository;
        _companyRepository = companyRepository;
        _movementRepository = movementRepository;
        _unitOfWork = unitOfWork;
        _validarOperadorEnOrden = validarOperadorEnOrden;
    }

    public Task<Result<ImportWsuOrderEpcsResult>> ImportOrderItemEpcsAsync(ImportWsuOrderEpcsInput request, CancellationToken cancellationToken = default)
        => _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            if (request.Items.Count == 0)
            {
                return Result<ImportWsuOrderEpcsResult>.Fail("At least one EPC is required.");
            }

            var order = await _orderRepository.GetByPublicIdAsync(request.CompanyPublicId, request.OrderPublicId, token);
            if (order is null)
            {
                return Result<ImportWsuOrderEpcsResult>.Fail("Order not found.");
            }

            if (await _orderItemEpcAssignmentRepository.AnyByOrderIdAsync(order.Id, token))
            {
                return Result<ImportWsuOrderEpcsResult>.Fail("Order already has EPC assignments.");
            }

            if (!order.WarehousePublicId.HasValue)
            {
                return Result<ImportWsuOrderEpcsResult>.Fail("Order has no warehouse and cannot import EPCs.");
            }

            var orderedItems = order.Items.OrderBy(item => item.Id).ToList();
            var expectedByItem = new List<(OrderItem Item, int RequiredCount)>(orderedItems.Count);
            var expectedTotal = 0;

            foreach (var item in orderedItems)
            {
                var requiredDecimal = Math.Abs(item.VariacionStock);
                if (requiredDecimal != decimal.Truncate(requiredDecimal))
                {
                    return Result<ImportWsuOrderEpcsResult>.Fail($"Order item '{item.PublicId}' has a non-integer VariacionStock and cannot be assigned by EPC count.");
                }

                var requiredCount = (int)requiredDecimal;
                if (requiredCount == 0)
                {
                    continue;
                }

                expectedByItem.Add((item, requiredCount));
                expectedTotal += requiredCount;
            }

            if (request.Items.Count != expectedTotal)
            {
                return Result<ImportWsuOrderEpcsResult>.Fail($"EPC count mismatch. Expected {expectedTotal} rows, received {request.Items.Count}.");
            }

            var epcs = request.Items.Select(item => item.Epc).ToArray();
            var warehousePublicId = order.WarehousePublicId.Value;
            var existingEpcs = await _rfidTagLookupRepository.ListExistingEpcsAsync(request.CompanyPublicId, warehousePublicId, epcs, token);
            if (existingEpcs.Count > 0)
            {
                return Result<ImportWsuOrderEpcsResult>.Fail(BuildExistingEpcsError(existingEpcs));
            }

            var missingEpcs = epcs
                .Where(item => !existingEpcs.Contains(item))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            await _rfidTagLookupRepository.AddMissingAsync(request.CompanyPublicId, warehousePublicId, missingEpcs, token);

            var assignments = new List<OrderItemEpcAssignment>(request.Items.Count);
            var pointer = 0;

            foreach (var itemEntry in expectedByItem)
            {
                if (!itemEntry.Item.ProductPublicId.HasValue)
                {
                    return Result<ImportWsuOrderEpcsResult>.Fail($"Order item '{itemEntry.Item.PublicId}' has no ProductPublicId.");
                }

                for (var index = 0; index < itemEntry.RequiredCount; index++)
                {
                    var line = request.Items[pointer++];
                    assignments.Add(OrderItemEpcAssignment.Create(order.Id, itemEntry.Item.Id, line.Epc));
                }
            }

            await _orderItemEpcAssignmentRepository.AddRangeAsync(assignments, token);
            order.MarkEpcLoadConfirmed();
            order.SetEpcSkuMatchCompleted(false);
            await _unitOfWork.SaveChangesAsync(token);

            return Result<ImportWsuOrderEpcsResult>.Ok(new ImportWsuOrderEpcsResult(expectedTotal, assignments.Count));
        }, cancellationToken);

    private static string BuildExistingEpcsError(IReadOnlySet<string> existingEpcs)
    {
        var ordered = existingEpcs
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (ordered.Length == 0)
        {
            return "Se encontraron EPCs ya registrados para la misma compañía y bodega.";
        }

        var shownCount = Math.Min(MaxDuplicateEpcsToShow, ordered.Length);
        var preview = string.Join(", ", ordered.Take(shownCount));
        var suffix = ordered.Length > shownCount ? ", ..." : string.Empty;

        return $"Se encontraron EPCs ya registrados para la misma compañía y bodega. EPCs duplicados (primeros {shownCount}): {preview}{suffix}. Total duplicados: {ordered.Length}.";
    }

    public Task<Result<Order>> CreateAsync(CreateWsuOrderInput request, CancellationToken cancellationToken = default)
    {
        if (request.Items.Count == 0)
        {
            return Task.FromResult(Result<Order>.Fail("At least one order item is required."));
        }

        var duplicateTripletError = ValidateUniqueSkuTriplets(request.Items);
        if (duplicateTripletError is not null)
        {
            return Task.FromResult(Result<Order>.Fail(duplicateTripletError));
        }

        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var company = await _companyRepository.GetByPublicIdAsync(request.CompanyPublicId, token);
            if (company is null)
            {
                return Result<Order>.Fail("Company not found.");
            }

            var order = Order.Create(
                request.CompanyPublicId,
                request.WarehousePublicId,
                request.OrderType,
                request.OrderNumber,
                request.ExternalOrderNumber,
                IsInboundOrderType(request.OrderType) ? OrderStatus.Draft : request.Status,
                request.MovementDate,
                request.CreatedByUserPublicId,
                request.SourceType,
                request.Notes);

            await _orderRepository.AddAsync(order, token);
            await _unitOfWork.SaveChangesAsync(token);

            var createdItems = new List<OrderItem>(request.Items.Count);
            var resolvedProductPublicIdsBySku = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);
            var resolvedUnitOfMeasureIdsBySymbol = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

            foreach (var input in request.Items)
            {
                var quantity = Math.Abs(input.VariacionStock);
                if (quantity <= 0m)
                {
                    return Result<Order>.Fail("VariacionStock must generate a positive Quantity.");
                }

                if (request.OrderType == OrderType.Adjustment && input.VariacionStock >= 0m)
                {
                    return Result<Order>.Fail("Adjustment orders require VariacionStock < 0.");
                }

                var resolvedProductPublicIdResult = await ResolveOrCreateProductPublicIdAsync(
                    company.Id,
                    input,
                    resolvedProductPublicIdsBySku,
                    resolvedUnitOfMeasureIdsBySymbol,
                    token);
                if (!resolvedProductPublicIdResult.Success)
                {
                    return Result<Order>.Fail(resolvedProductPublicIdResult.Error!);
                }

                // Pricing rule for Sprint 1 snapshot:
                // Consumption uses PrecioVentaUnitario; Inbound/Replenishment use PrecioCompraUnitario;
                // Adjustment is a loss/theft flow so UnitPrice and LineTotal are left null.
                var item = OrderItem.Create(
                    order.Id,
                    resolvedProductPublicIdResult.Value,
                    0m,
                    input.SkuWsu,
                    input.NombreSkuWsu,
                    input.SkuProveedor,
                    input.NombreSkuProveedor,
                    input.RutProveedor,
                    input.RsProveedor,
                    input.SkuCliente,
                    input.NombreSkuCliente,
                    input.RutCliente,
                    input.RsCliente,
                    input.Posicion,
                    input.UnidadDeMedida,
                    input.LoteMinimoCompra,
                    input.LargoCompraCm,
                    input.AnchoCompraCm,
                    input.AltoCompraCm,
                    input.PesoCompraKg,
                    input.ApilableCompra,
                    input.TipoAlmacenamientoCompra,
                    input.LoteMinimoVenta,
                    input.LargoVentaCm,
                    input.AnchoVentaCm,
                    input.AltoVentaCm,
                    input.PesoVentaKg,
                    input.ApilableVenta,
                    input.TipoAlmacenamientoVenta,
                    input.PrecioCompraUnitario,
                    input.PrecioVentaUnitario,
                    input.VariacionStock,
                    input.StockMinimo);

                await _orderItemRepository.AddAsync(item, token);
                createdItems.Add(item);
            }

            await _unitOfWork.SaveChangesAsync(token);

            for (var index = 0; index < createdItems.Count; index++)
            {
                var input = request.Items[index];
                var item = createdItems[index];
                var quantity = item.GetMovementQuantity();
                var signedQuantity = request.OrderType is OrderType.Consumption or OrderType.Adjustment
                    ? -quantity
                    : quantity;

                if (IsInboundOrderType(request.OrderType))
                {
                    continue;
                }

                var movement = WsuInventoryMovement.Create(
                    request.CompanyPublicId,
                    request.WarehousePublicId,
                    order.Id,
                    item.Id,
                    item.ProductPublicId,
                    ResolveMovementType(request.OrderType),
                    quantity,
                    signedQuantity,
                    request.MovementDate,
                    request.CreatedByUserPublicId,
                    request.SourceType,
                    input.Notes ?? request.Notes);

                await _movementRepository.AddAsync(movement, token);

                if (IsOutboundOrderType(request.OrderType))
                {
                    if (!item.ProductPublicId.HasValue || item.ProductPublicId.Value == Guid.Empty)
                    {
                        return Result<Order>.Fail("Outbound order items require ProductPublicId for FIFO costing.");
                    }

                    var fifoResult = await ApplyFifoForOutboundItemAsync(
                        request.CompanyPublicId,
                        request.WarehousePublicId,
                        item,
                        token);

                    if (!fifoResult.Success)
                    {
                        return Result<Order>.Fail(fifoResult.Error!);
                    }
                }
            }

            order.Touch();
            await _unitOfWork.SaveChangesAsync(token);

            return Result<Order>.Ok(order);
        }, cancellationToken);
    }

    public Task<Result<Order>> ReconcileInboundWithRfidAsync(ReconcileWsuOrderInput request, CancellationToken cancellationToken = default)
        => _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            if (request.Items.Count == 0)
            {
                return Result<Order>.Fail("At least one reconciliation item is required.");
            }

            var order = await _orderRepository.GetByPublicIdAsync(request.CompanyPublicId, request.OrderPublicId, token);
            if (order is null)
            {
                return Result<Order>.Fail("Order not found.");
            }

            if (!IsInboundOrderType(order.OrderType))
            {
                return Result<Order>.Fail("Only inbound or replenishment orders can be reconciled with RFID.");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                return Result<Order>.Fail("Cancelled orders cannot be reconciled.");
            }

            if (order.Status != OrderStatus.Confirmed)
            {
                return Result<Order>.Fail("Only confirmed orders can be reconciled with RFID.");
            }

            var orderItemsByPublicId = order.Items.ToDictionary(item => item.PublicId, item => item);
            var reconciledAt = request.ReconciledAt ?? DateTimeOffset.UtcNow;

            foreach (var input in request.Items)
            {
                if (input.OrderItemPublicId == Guid.Empty)
                {
                    return Result<Order>.Fail("OrderItemPublicId is required.");
                }

                if (input.ReconciledQuantity <= 0m)
                {
                    return Result<Order>.Fail("ReconciledQuantity must be greater than zero.");
                }

                if (!orderItemsByPublicId.TryGetValue(input.OrderItemPublicId, out var item))
                {
                    return Result<Order>.Fail($"Order item '{input.OrderItemPublicId}' does not belong to order '{order.PublicId}'.");
                }

                item.ReconcileInboundQuantity(input.ReconciledQuantity);

                var reconciliation = OrderItemReconciliation.Create(
                    order.Id,
                    item.Id,
                    input.ReconciledQuantity,
                    reconciledAt,
                    request.ReconciledByUserPublicId,
                    SourceType.Rfid,
                    request.Notes);

                await _orderItemReconciliationRepository.AddAsync(reconciliation, token);

                var movement = WsuInventoryMovement.Create(
                    order.CompanyPublicId,
                    order.WarehousePublicId,
                    order.Id,
                    item.Id,
                    item.ProductPublicId,
                    WsuMovementType.Inbound,
                    input.ReconciledQuantity,
                    input.ReconciledQuantity,
                    reconciledAt,
                    request.ReconciledByUserPublicId,
                    SourceType.Rfid,
                    request.Notes ?? order.Notes);

                await _movementRepository.AddAsync(movement, token);
            }

            order.Touch();
            await _unitOfWork.SaveChangesAsync(token);
            return Result<Order>.Ok(order);
        }, cancellationToken);

    public Task<Result<Order>> ConfirmOrderAsync(ChangeWsuOrderStatusInput request, CancellationToken cancellationToken = default)
        => _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var order = await _orderRepository.GetByPublicIdAsync(request.CompanyPublicId, request.OrderPublicId, token);
            if (order is null)
            {
                return Result<Order>.Fail("Order not found.");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                return Result<Order>.Fail("Cancelled orders cannot be confirmed.");
            }

            if (order.Status == OrderStatus.Confirmed)
            {
                return Result<Order>.Ok(order);
            }

            if (order.Status != OrderStatus.Draft)
            {
                return Result<Order>.Fail("Only draft orders can be confirmed.");
            }

            if (_validarOperadorEnOrden)
            {
                var operatorCodes = await _orderMovementOperatorRepository.ListOperatorCodesByOrderIdAsync(order.Id, token);
                if (operatorCodes.Count == 0)
                {
                    return Result<Order>.Fail("At least one operator must be associated with the order before confirming this order.");
                }

                foreach (var operatorCode in operatorCodes)
                {
                    var isEnrolled = await _operatorEnrollmentValidator.IsEnrolledAsync(request.CompanyPublicId, operatorCode, token);
                    if (!isEnrolled)
                    {
                        return Result<Order>.Fail($"Operator '{operatorCode}' is not enrolled in RFID (active credential with FaceTemplateId and NfcCardUid is required).");
                    }
                }
            }

            order.SetStatus(OrderStatus.Confirmed);
            order.Touch();
            await _unitOfWork.SaveChangesAsync(token);
            return Result<Order>.Ok(order);
        }, cancellationToken);

    public Task<Result<Order>> CancelOrderAsync(ChangeWsuOrderStatusInput request, CancellationToken cancellationToken = default)
        => _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var order = await _orderRepository.GetByPublicIdAsync(request.CompanyPublicId, request.OrderPublicId, token);
            if (order is null)
            {
                return Result<Order>.Fail("Order not found.");
            }

            if (order.Status == OrderStatus.Cancelled)
            {
                return Result<Order>.Ok(order);
            }

            if (order.Status != OrderStatus.Draft)
            {
                return Result<Order>.Fail("Only draft orders can be cancelled.");
            }

            var assignedEpcs = await _orderItemEpcAssignmentRepository.ListEpcsByOrderIdAsync(order.Id, token);
            if (assignedEpcs.Count > 0)
            {
                await _orderItemEpcAssignmentRepository.DeleteByOrderIdAsync(order.Id, token);

                if (order.WarehousePublicId.HasValue)
                {
                    await _rfidTagLookupRepository.DeleteByCompanyWarehouseAndEpcsAsync(
                        request.CompanyPublicId,
                        order.WarehousePublicId.Value,
                        assignedEpcs,
                        token);
                }
            }

            order.ResetEpcLoadConfirmed();
            order.SetEpcSkuMatchCompleted(false);

            order.SetStatus(OrderStatus.Cancelled);
            order.Touch();
            await _unitOfWork.SaveChangesAsync(token);
            return Result<Order>.Ok(order);
        }, cancellationToken);

    public Task<Result<Order>> AssignMovementOperatorsAsync(AssignWsuOrderMovementOperatorsInput request, CancellationToken cancellationToken = default)
        => _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            if (request.Items.Count == 0)
            {
                return Result<Order>.Fail("At least one movement operator is required.");
            }

            var order = await _orderRepository.GetByPublicIdAsync(request.CompanyPublicId, request.OrderPublicId, token);
            if (order is null)
            {
                return Result<Order>.Fail("Order not found.");
            }

            var normalizedInputs = new List<AssignWsuOrderMovementOperatorInput>(request.Items.Count);
            var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in request.Items)
            {
                if (string.IsNullOrWhiteSpace(item.CodigoOperador))
                {
                    return Result<Order>.Fail("CodigoOperador is required for every movement operator item.");
                }

                var normalizedCode = item.CodigoOperador.Trim();
                if (!seenCodes.Add(normalizedCode))
                {
                    return Result<Order>.Fail($"Duplicate CodigoOperador '{normalizedCode}' is not allowed in request items.");
                }

                normalizedInputs.Add(new AssignWsuOrderMovementOperatorInput(
                    normalizedCode,
                    item.NombreOperador,
                    item.FechaMovimiento,
                    item.HoraInicioMovimiento,
                    item.HoraFinMovimiento));
            }

            await _orderMovementOperatorRepository.DeleteByOrderIdAsync(order.Id, token);

            var newRows = new List<WsuOrderMovementOperator>(normalizedInputs.Count);
            foreach (var operatorInput in normalizedInputs)
            {
                newRows.Add(WsuOrderMovementOperator.Create(
                    order.Id,
                    operatorInput.CodigoOperador,
                    operatorInput.NombreOperador,
                    operatorInput.FechaMovimiento,
                    operatorInput.HoraInicioMovimiento,
                    operatorInput.HoraFinMovimiento));
            }

            await _orderMovementOperatorRepository.AddRangeAsync(newRows, token);
            order.MarkMovementProgrammed();
            await _unitOfWork.SaveChangesAsync(token);
            return Result<Order>.Ok(order);
        }, cancellationToken);

    private static WsuMovementType ResolveMovementType(OrderType orderType)
        => orderType is OrderType.Consumption or OrderType.Adjustment
            ? WsuMovementType.Outbound
            : WsuMovementType.Inbound;

    private async Task<Result> ApplyFifoForOutboundItemAsync(
        Guid companyPublicId,
        Guid? warehousePublicId,
        OrderItem outItem,
        CancellationToken cancellationToken)
    {
        var availableInboundItems = await _orderItemRepository.ListAvailableInboundForFifoAsync(
            companyPublicId,
            warehousePublicId,
            outItem.ProductPublicId!.Value,
            cancellationToken);

        var remaining = outItem.GetMovementQuantity();
        foreach (var inboundItem in availableInboundItems)
        {
            if (remaining <= 0m)
            {
                break;
            }

            var consumeQuantity = Math.Min(remaining, inboundItem.QuantityAvailableForFifo);
            if (consumeQuantity <= 0m)
            {
                continue;
            }

            var unitCost = inboundItem.PrecioCompraUnitario ?? 0m;
            inboundItem.ConsumeQuantity(consumeQuantity);

            var consumption = OrderItemConsumption.Create(
                outItem.Id,
                inboundItem.Id,
                consumeQuantity,
                unitCost);

            await _orderItemConsumptionRepository.AddAsync(consumption, cancellationToken);
            remaining -= consumeQuantity;
        }

        if (remaining > 0m)
        {
            return Result.Fail($"Insufficient stock for product '{outItem.ProductPublicId}'. Missing quantity: {remaining}.");
        }

        return Result.Ok();
    }

    private static bool IsInboundOrderType(OrderType orderType)
        => orderType is OrderType.Inbound or OrderType.Replenishment;

    private static bool IsOutboundOrderType(OrderType orderType)
        => orderType is OrderType.Consumption or OrderType.Adjustment;

    private static string? ValidateUniqueSkuTriplets(IReadOnlyList<CreateWsuOrderItemInput> items)
    {
        var firstIndexByTriplet = new Dictionary<string, int>(StringComparer.Ordinal);

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var tripletKey = BuildSkuTripletKey(item.SkuWsu, item.SkuProveedor, item.SkuCliente);

            if (firstIndexByTriplet.TryGetValue(tripletKey, out var firstIndex))
            {
                return $"Duplicate SKU triplet is not allowed. Rows {firstIndex + 1} and {index + 1} have the same SkuWsu/SkuProveedor/SkuCliente combination.";
            }

            firstIndexByTriplet[tripletKey] = index;
        }

        return null;
    }

    private static string BuildSkuTripletKey(string? skuWsu, string? skuProveedor, string? skuCliente)
        => string.Join("|", NormalizeSkuValue(skuWsu), NormalizeSkuValue(skuProveedor), NormalizeSkuValue(skuCliente));

    private static string NormalizeSkuValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    private async Task<Result<Guid?>> ResolveOrCreateProductPublicIdAsync(
        long companyId,
        CreateWsuOrderItemInput input,
        IDictionary<string, Guid> resolvedProductPublicIdsBySku,
        IDictionary<string, long> resolvedUnitOfMeasureIdsBySymbol,
        CancellationToken cancellationToken)
    {
        if (input.ProductPublicId.HasValue && input.ProductPublicId.Value != Guid.Empty)
        {
            var existingByPublicId = await _productRepository.GetByPublicIdAsync(companyId, input.ProductPublicId.Value, cancellationToken);
            if (existingByPublicId is not null)
            {
                if (!string.IsNullOrWhiteSpace(existingByPublicId.Sku))
                {
                    resolvedProductPublicIdsBySku[existingByPublicId.Sku] = existingByPublicId.PublicId;
                }

                return Result<Guid?>.Ok(existingByPublicId.PublicId);
            }
        }

        if (string.IsNullOrWhiteSpace(input.SkuWsu))
        {
            return Result<Guid?>.Fail("ProductPublicId not found and SkuWsu is required to resolve or create product.");
        }

        var normalizedSku = input.SkuWsu.Trim();
        if (resolvedProductPublicIdsBySku.TryGetValue(normalizedSku, out var cachedProductPublicId))
        {
            return Result<Guid?>.Ok(cachedProductPublicId);
        }

        var existingBySku = await _productRepository.GetBySkuAsync(companyId, normalizedSku, cancellationToken);
        if (existingBySku is not null)
        {
            resolvedProductPublicIdsBySku[normalizedSku] = existingBySku.PublicId;
            return Result<Guid?>.Ok(existingBySku.PublicId);
        }

        if (string.IsNullOrWhiteSpace(input.UnidadDeMedida))
        {
            return Result<Guid?>.Fail("Cannot auto-create product because UnidadDeMedida is required.");
        }

        var normalizedUnitSymbol = input.UnidadDeMedida.Trim();
        if (!resolvedUnitOfMeasureIdsBySymbol.TryGetValue(normalizedUnitSymbol, out var resolvedUnitOfMeasureId))
        {
            var enabledCompanyUnits = await _companyUnitOfMeasureRepository.ListByCompanyAsync(companyId, enabledOnly: true, cancellationToken);

            var matchedCompanyUnit = enabledCompanyUnits.FirstOrDefault(item =>
                item.UnitOfMeasure?.Translations.Any(translation =>
                    !string.IsNullOrWhiteSpace(translation.Symbol)
                    && string.Equals(translation.Symbol!.Trim(), normalizedUnitSymbol, StringComparison.OrdinalIgnoreCase)) == true);

            if (matchedCompanyUnit is null)
            {
                return Result<Guid?>.Fail($"Cannot auto-create product because UnidadDeMedida '{normalizedUnitSymbol}' is not configured for the company.");
            }

            resolvedUnitOfMeasureId = matchedCompanyUnit.UnitOfMeasureId;
            resolvedUnitOfMeasureIdsBySymbol[normalizedUnitSymbol] = resolvedUnitOfMeasureId;
        }

        var resolvedName = string.IsNullOrWhiteSpace(input.NombreSkuWsu)
            ? normalizedSku
            : input.NombreSkuWsu.Trim();

        var product = Product.Create(
            companyId,
            normalizedSku,
            resolvedName,
            barcode: null,
            unitOfMeasureId: resolvedUnitOfMeasureId,
            isStockable: true,
            isSellable: true,
            isPurchasable: true,
            isStackable: input.ApilableCompra ?? input.ApilableVenta,
            lengthCm: input.LargoCompraCm ?? input.LargoVentaCm,
            widthCm: input.AnchoCompraCm ?? input.AnchoVentaCm,
            weightKg: input.PesoCompraKg ?? input.PesoVentaKg,
            storageType: input.TipoAlmacenamientoCompra ?? input.TipoAlmacenamientoVenta);

        await _productRepository.AddAsync(product, cancellationToken);
        resolvedProductPublicIdsBySku[normalizedSku] = product.PublicId;
        return Result<Guid?>.Ok(product.PublicId);
    }
}
