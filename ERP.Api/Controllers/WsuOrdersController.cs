using ERP.Api.Authorization;
using ERP.Api.Contracts.Wsu;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Modules.Wsu.Application.Repositories;
using ERP.Modules.Wsu.Application.Services;
using ERP.Modules.Wsu.Domain;
using ERP.Shared.Application;
using ClosedXML.Excel;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/wsu/orders")]
public sealed class WsuOrdersController : ControllerBase
{
    private const long MaxImportFileBytes = 10 * 1024 * 1024;

    private readonly WsuOrderService _wsuOrderService;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _orderItemRepository;
    private readonly IOrderItemConsumptionRepository _orderItemConsumptionRepository;
    private readonly IOrderItemEpcAssignmentRepository _orderItemEpcAssignmentRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IRbacService _rbacService;
    private readonly IUnitOfWork _unitOfWork;

    public WsuOrdersController(
        WsuOrderService wsuOrderService,
        IOrderRepository orderRepository,
        IOrderItemRepository orderItemRepository,
        IOrderItemConsumptionRepository orderItemConsumptionRepository,
        IOrderItemEpcAssignmentRepository orderItemEpcAssignmentRepository,
        ICurrentUserProvider currentUserProvider,
        IRbacService rbacService,
        IUnitOfWork unitOfWork)
    {
        _wsuOrderService = wsuOrderService;
        _orderRepository = orderRepository;
        _orderItemRepository = orderItemRepository;
        _orderItemConsumptionRepository = orderItemConsumptionRepository;
        _orderItemEpcAssignmentRepository = orderItemEpcAssignmentRepository;
        _currentUserProvider = currentUserProvider;
        _rbacService = rbacService;
        _unitOfWork = unitOfWork;
    }

    [HttpPost]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> Create(CreateWsuOrderRequest request, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(request.CompanyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        if (string.IsNullOrWhiteSpace(request.OrderNumber))
        {
            return BadRequest(new { error = "OrderNumber is required." });
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest(new { error = "At least one order item is required." });
        }

        var movementDate = request.MovementDate ?? DateTimeOffset.UtcNow;

        var input = new CreateWsuOrderInput(
            companyResolution.CompanyPublicId,
            request.WarehousePublicId,
            request.OrderType,
            request.OrderNumber,
            request.ExternalOrderNumber,
            request.Status,
            movementDate,
            request.CreatedByUserPublicId,
            request.SourceType,
            request.Notes,
            request.Items.Select(item => new CreateWsuOrderItemInput(
                item.ProductPublicId,
                item.SkuWsu,
                item.NombreSkuWsu,
                item.SkuProveedor,
                item.NombreSkuProveedor,
                item.RutProveedor,
                item.RsProveedor,
                item.SkuCliente,
                item.NombreSkuCliente,
                item.RutCliente,
                item.RsCliente,
                item.Posicion,
                item.UnidadDeMedida,
                item.LoteMinimoCompra,
                item.LargoCompraCm,
                item.AnchoCompraCm,
                item.AltoCompraCm,
                item.PesoCompraKg,
                item.ApilableCompra,
                item.TipoAlmacenamientoCompra,
                item.LoteMinimoVenta,
                item.LargoVentaCm,
                item.AnchoVentaCm,
                item.AltoVentaCm,
                item.PesoVentaKg,
                item.ApilableVenta,
                item.TipoAlmacenamientoVenta,
                item.PrecioCompraUnitario,
                item.PrecioVentaUnitario,
                item.VariacionStock,
                item.StockMinimo,
                item.Notes)).ToList());

        var result = await _wsuOrderService.CreateAsync(input, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        var response = ToResponse(result.Value!);
        return CreatedAtAction(nameof(GetByPublicId), new { publicId = response.PublicId, companyPublicId = response.CompanyPublicId }, response);
    }

    [HttpPost("{publicId:guid}/reconcile-rfid")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> ReconcileRfid(Guid publicId, ReconcileWsuOrderRfidRequest request, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(request.CompanyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest(new { error = "At least one reconciliation item is required." });
        }

        var input = new ReconcileWsuOrderInput(
            companyResolution.CompanyPublicId,
            publicId,
            request.ReconciledAt,
            request.ReconciledByUserPublicId,
            request.Notes,
            request.Items.Select(item => new ReconcileWsuOrderItemInput(item.OrderItemPublicId, item.ReconciledQuantity)).ToList());

        var result = await _wsuOrderService.ReconcileInboundWithRfidAsync(input, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(ToDetailResponse(result.Value!));
    }

    [HttpPost("{publicId:guid}/confirm")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> Confirm(Guid publicId, ChangeWsuOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(request.CompanyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        var input = new ChangeWsuOrderStatusInput(
            companyResolution.CompanyPublicId,
            publicId,
            request.UserPublicId,
            request.Notes);

        var result = await _wsuOrderService.ConfirmOrderAsync(input, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(ToDetailResponse(result.Value!));
    }

    [HttpPost("{publicId:guid}/cancel")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> Cancel(Guid publicId, ChangeWsuOrderStatusRequest request, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(request.CompanyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        var input = new ChangeWsuOrderStatusInput(
            companyResolution.CompanyPublicId,
            publicId,
            request.UserPublicId,
            request.Notes);

        var result = await _wsuOrderService.CancelOrderAsync(input, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(ToDetailResponse(result.Value!));
    }

    [HttpPost("{publicId:guid}/movement-operators")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> AssignMovementOperators(Guid publicId, AssignWsuOrderMovementOperatorsRequest request, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(request.CompanyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest(new { error = "At least one movement operator is required." });
        }

        var input = new AssignWsuOrderMovementOperatorsInput(
            companyResolution.CompanyPublicId,
            publicId,
            request.Items.Select(item => new AssignWsuOrderMovementOperatorInput(
                item.CodigoOperador,
                item.NombreOperador,
                item.FechaMovimiento,
                item.HoraInicioMovimiento,
                item.HoraFinMovimiento)).ToList());

        var result = await _wsuOrderService.AssignMovementOperatorsAsync(input, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(ToDetailResponse(result.Value!));
    }

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> ImportExcel([FromForm] ImportWsuOrderExcelRequest request, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(request.CompanyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        if (request.File.Length > MaxImportFileBytes)
        {
            return BadRequest(new { error = $"File exceeds max size of {MaxImportFileBytes / (1024 * 1024)} MB." });
        }

        if (!string.Equals(Path.GetExtension(request.File.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only .xlsx files are supported." });
        }

        List<CreateWsuOrderItemInput> items;
        List<object> rowErrors;

        await using (var stream = request.File.OpenReadStream())
        {
            (items, rowErrors) = ParseOrderItemsFromExcel(stream, request.OrderType);
        }

        if (rowErrors.Count > 0)
        {
            return BadRequest(new { error = "Invalid spreadsheet data.", rowErrors });
        }

        if (items.Count == 0)
        {
            return BadRequest(new { error = "No valid data rows were found in the spreadsheet." });
        }

        var movementDate = request.MovementDate ?? DateTimeOffset.UtcNow;
        var input = new CreateWsuOrderInput(
            companyResolution.CompanyPublicId,
            request.WarehousePublicId,
            request.OrderType,
            GenerateImportOrderNumber(),
            request.ExternalOrderNumber,
            request.Status ?? OrderStatus.Confirmed,
            movementDate,
            CreatedByUserPublicId: null,
            request.SourceType ?? SourceType.Import,
            request.Notes,
            items);

        var result = await _wsuOrderService.CreateAsync(input, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        var response = ToResponse(result.Value!);
        return CreatedAtAction(nameof(GetByPublicId), new { publicId = response.PublicId, companyPublicId = response.CompanyPublicId }, response);
    }

    [HttpPost("{publicId:guid}/import-epc")]
    [Consumes("multipart/form-data")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> ImportEpcs(Guid publicId, [FromForm] ImportWsuOrderEpcExcelRequest request, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(request.CompanyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { error = "File is required." });
        }

        if (request.File.Length > MaxImportFileBytes)
        {
            return BadRequest(new { error = $"File exceeds max size of {MaxImportFileBytes / (1024 * 1024)} MB." });
        }

        if (!string.Equals(Path.GetExtension(request.File.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Only .xlsx files are supported." });
        }

        List<ImportWsuOrderEpcLineInput> epcs;
        List<object> rowErrors;

        await using (var stream = request.File.OpenReadStream())
        {
            (epcs, rowErrors) = ParseOrderEpcsFromExcel(stream);
        }

        if (rowErrors.Count > 0)
        {
            return BadRequest(new { error = "Invalid spreadsheet data.", rowErrors });
        }

        if (epcs.Count == 0)
        {
            return BadRequest(new { error = "No valid data rows were found in the spreadsheet." });
        }

        var input = new ImportWsuOrderEpcsInput(companyResolution.CompanyPublicId, publicId, epcs);
        var result = await _wsuOrderService.ImportOrderItemEpcsAsync(input, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new WsuOrderEpcImportResponse(publicId, result.Value!.ExpectedTags, result.Value.ImportedTags));
    }

    [HttpPatch("{publicId:guid}/epc-checks")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> UpdateEpcChecks(Guid publicId, UpdateWsuOrderEpcChecksRequest request, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(request.CompanyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest(new { error = "At least one EPC check update is required." });
        }

        if (!request.Items.Any(item => !string.IsNullOrWhiteSpace(item.Epc)))
        {
            return BadRequest(new { error = "At least one EPC with a non-empty value is required." });
        }

        var order = await _orderRepository.GetByPublicIdAsync(companyResolution.CompanyPublicId, publicId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        var updates = request.Items
            .Select(item => new OrderItemEpcCheckUpdateItem(item.Epc, item.IsChecked))
            .ToArray();

        var updateResult = await _orderItemEpcAssignmentRepository.ApplyChecksAsync(order.Id, updates, cancellationToken);
        order.SetEpcSkuMatchCompleted(updateResult.IsEpcSkuMatchCompleted);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new UpdateWsuOrderEpcChecksResponse(
            publicId,
            updateResult.UpdatedCount,
            updateResult.TotalAssignments,
            updateResult.CheckedAssignments,
            updateResult.IsEpcSkuMatchCompleted,
            updateResult.NotFoundEpcs));
    }

    [HttpGet("{publicId:guid}/epc-assignments")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> ListEpcAssignments(Guid publicId, [FromQuery] Guid? companyPublicId, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(companyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        var order = await _orderRepository.GetByPublicIdAsync(companyResolution.CompanyPublicId, publicId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        var items = await _orderItemEpcAssignmentRepository.ListDetailedByOrderIdAsync(order.Id, cancellationToken);
        var response = items
            .Select(item => new WsuOrderEpcAssignmentListItemResponse(
                item.SkuWsu,
                item.NombreSkuWsu,
                item.SkuProveedor,
                item.NombreSkuProveedor,
                item.SkuCliente,
                item.NombreSkuCliente,
                item.Epc,
                item.IsChecked))
            .ToList();

        return Ok(response);
    }

    [HttpGet("import/template")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public IActionResult DownloadImportTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("WSU_Import");
        var headers = GetTemplateHeaders();

        for (var index = 0; index < headers.Length; index++)
        {
            worksheet.Cell(1, index + 1).Value = headers[index];
            worksheet.Cell(1, index + 1).Style.Font.Bold = true;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "PlantillaCargaWsu.xlsx");
    }

    [HttpGet("{publicId:guid}")]
    public async Task<IActionResult> GetByPublicId(Guid publicId, [FromQuery] Guid? companyPublicId, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(companyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        var item = await _orderRepository.GetByPublicIdAsync(companyResolution.CompanyPublicId, publicId, cancellationToken);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(ToDetailResponse(item));
    }

    [HttpGet]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> List(
        [FromQuery] Guid? companyPublicId,
        [FromQuery] Guid? warehousePublicId,
        [FromQuery] OrderType? orderType,
        [FromQuery] DateTimeOffset? movementDateFrom,
        [FromQuery] DateTimeOffset? movementDateTo,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(companyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        var normalizedSkip = Math.Max(0, skip);
        var normalizedTake = Math.Clamp(take <= 0 ? 50 : take, 1, 200);

        var items = await _orderRepository.ListAsync(
            companyResolution.CompanyPublicId,
            warehousePublicId,
            orderType,
            movementDateFrom,
            movementDateTo,
            normalizedSkip,
            normalizedTake,
            cancellationToken);

        var total = await _orderRepository.CountAsync(
            companyResolution.CompanyPublicId,
            warehousePublicId,
            orderType,
            movementDateFrom,
            movementDateTo,
            cancellationToken);

        var response = items.Select(item => new WsuOrderListItemResponse(
            item.Id,
            item.PublicId,
            item.OrderType,
            item.OrderNumber,
            item.ExternalOrderNumber,
            item.Status,
            item.MovementDate,
            item.CreatedByUserPublicId,
            item.SourceType,
            item.Notes,
            item.IsEpcLoadConfirmed,
            item.IsMovementProgrammed,
            item.IsEpcSkuMatchCompleted,
            item.ItemsCount,
            item.TagsACargar,
            item.CreatedAt,
            item.UpdatedAt,
            item.WarehouseCode,
            item.WarehouseName,
            item.WarehouseLocation)).ToList();

        return Ok(new PagedResult<WsuOrderListItemResponse>(response, total, normalizedSkip, normalizedTake));
    }

    [HttpGet("stock")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> GetAvailableStock(
        [FromQuery] Guid productPublicId,
        [FromQuery] Guid? warehousePublicId,
        [FromQuery] Guid? companyPublicId,
        CancellationToken cancellationToken)
    {
        if (productPublicId == Guid.Empty)
        {
            return BadRequest(new { error = "productPublicId is required." });
        }

        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(companyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        var quantityAvailable = await _orderItemRepository.GetAvailableStockAsync(
            companyResolution.CompanyPublicId,
            warehousePublicId,
            productPublicId,
            cancellationToken);

        return Ok(new WsuStockAvailabilityResponse(
            companyResolution.CompanyPublicId,
            warehousePublicId,
            productPublicId,
            quantityAvailable));
    }

    [HttpGet("valuation")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> GetInventoryValuation(
        [FromQuery] Guid? warehousePublicId,
        [FromQuery] Guid? companyPublicId,
        CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(companyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        var valuation = await _orderItemRepository.GetInventoryValuationAsync(
            companyResolution.CompanyPublicId,
            warehousePublicId,
            cancellationToken);

        return Ok(new WsuInventoryValuationResponse(
            companyResolution.CompanyPublicId,
            warehousePublicId,
            valuation));
    }

    [HttpGet("{publicId:guid}/fifo")]
    [RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
    public async Task<IActionResult> GetFifoDetail(Guid publicId, [FromQuery] Guid? companyPublicId, CancellationToken cancellationToken)
    {
        var companyResolution = await ResolveEffectiveCompanyPublicIdAsync(companyPublicId, cancellationToken);
        if (!companyResolution.Success)
        {
            return companyResolution.Error!;
        }

        var order = await _orderRepository.GetByPublicIdAsync(companyResolution.CompanyPublicId, publicId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        if (order.OrderType is not OrderType.Consumption and not OrderType.Adjustment)
        {
            return BadRequest(new { error = "FIFO detail applies only to outbound orders." });
        }

        var outItems = order.Items
            .OrderBy(item => item.Id)
            .ToList();

        var itemDetails = new List<WsuFifoOutItemDetailResponse>(outItems.Count);
        decimal totalAppliedCost = 0m;

        foreach (var outItem in outItems)
        {
            var consumptions = await _orderItemConsumptionRepository.ListByOutOrderItemIdAsync(outItem.Id, cancellationToken);
            var lines = consumptions
                .Select(consumption => new WsuFifoConsumptionLineResponse(
                    consumption.Id,
                    consumption.InOrderItem.PublicId,
                    consumption.InOrderItem.Order.OrderNumber,
                    consumption.InOrderItem.Order.MovementDate,
                    consumption.Quantity,
                    consumption.UnitCost,
                    consumption.TotalCost))
                .ToList();

            var appliedCost = lines.Sum(line => line.TotalCost);
            totalAppliedCost += appliedCost;

            itemDetails.Add(new WsuFifoOutItemDetailResponse(
                outItem.PublicId,
                outItem.ProductPublicId,
                outItem.GetMovementQuantity(),
                appliedCost,
                lines));
        }

        return Ok(new WsuOrderFifoDetailResponse(
            order.PublicId,
            order.OrderNumber,
            order.MovementDate,
            totalAppliedCost,
            itemDetails));
    }

    private async Task<(bool Success, Guid CompanyPublicId, IActionResult? Error)> ResolveEffectiveCompanyPublicIdAsync(
        Guid? requestedCompanyPublicId,
        CancellationToken cancellationToken)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null || currentUser.UserId <= 0)
        {
            return (false, default, Unauthorized());
        }

        if (requestedCompanyPublicId.HasValue && requestedCompanyPublicId.Value == Guid.Empty)
        {
            return (false, default, BadRequest(new { error = "companyPublicId must be a valid guid." }));
        }

        if (currentUser.CompanyPublicId is not Guid tokenCompanyPublicId || tokenCompanyPublicId == Guid.Empty)
        {
            if (!requestedCompanyPublicId.HasValue)
            {
                return (false, default, Unauthorized());
            }

            var hasPlatformWsuManagePermissionWithoutTenant = await _rbacService.HasPermissionAsync(
                currentUser.UserId,
                PermissionKeys.Platform.WsuManage,
                RoleAssignmentScopeType.Platform,
                organizationId: null,
                companyPublicId: null,
                ct: cancellationToken);
            if (!hasPlatformWsuManagePermissionWithoutTenant)
            {
                return (false, default, Forbid());
            }

            return (true, requestedCompanyPublicId.Value, null);
        }

        if (!requestedCompanyPublicId.HasValue || requestedCompanyPublicId.Value == tokenCompanyPublicId)
        {
            return (true, tokenCompanyPublicId, null);
        }

        var hasPlatformWsuManagePermission = await _rbacService.HasPermissionAsync(
            currentUser.UserId,
            PermissionKeys.Platform.WsuManage,
            RoleAssignmentScopeType.Platform,
            organizationId: null,
            companyPublicId: null,
            ct: cancellationToken);
        if (!hasPlatformWsuManagePermission)
        {
            return (false, default, Forbid());
        }

        return (true, requestedCompanyPublicId.Value, null);
    }

    private static WsuOrderResponse ToResponse(Order item)
        => new(
            item.Id,
            item.PublicId,
            item.CompanyPublicId,
            item.WarehousePublicId,
            item.OrderType,
            item.OrderNumber,
            item.ExternalOrderNumber,
            item.Status,
            item.MovementDate,
            item.CreatedByUserPublicId,
            item.SourceType,
            item.Notes,
            item.IsEpcLoadConfirmed,
            item.Items.Count,
            item.CreatedAt,
            item.UpdatedAt);

    private static WsuOrderDetailResponse ToDetailResponse(Order item)
        => new(
            item.Id,
            item.PublicId,
            item.CompanyPublicId,
            item.WarehousePublicId,
            item.OrderType,
            item.OrderNumber,
            item.ExternalOrderNumber,
            item.Status,
            item.MovementDate,
            item.CreatedByUserPublicId,
            item.SourceType,
            item.Notes,
            item.IsEpcLoadConfirmed,
            item.IsMovementProgrammed,
            item.IsEpcSkuMatchCompleted,
            item.CreatedAt,
            item.UpdatedAt,
            item.Items
                .OrderBy(orderItem => orderItem.Id)
                .Select(orderItem => new WsuOrderDetailItemResponse(
                    orderItem.Id,
                    orderItem.PublicId,
                    orderItem.ProductPublicId,
                    orderItem.SkuWsu,
                    orderItem.NombreSkuWsu,
                    orderItem.SkuProveedor,
                    orderItem.NombreSkuProveedor,
                    orderItem.RutProveedor,
                    orderItem.RsProveedor,
                    orderItem.SkuCliente,
                    orderItem.NombreSkuCliente,
                    orderItem.RutCliente,
                    orderItem.RsCliente,
                    orderItem.Posicion,
                    orderItem.UnidadDeMedida,
                    orderItem.LoteMinimoCompra,
                    orderItem.LargoCompraCm,
                    orderItem.AnchoCompraCm,
                    orderItem.AltoCompraCm,
                    orderItem.PesoCompraKg,
                    orderItem.ApilableCompra,
                    orderItem.TipoAlmacenamientoCompra,
                    orderItem.LoteMinimoVenta,
                    orderItem.LargoVentaCm,
                    orderItem.AnchoVentaCm,
                    orderItem.AltoVentaCm,
                    orderItem.PesoVentaKg,
                    orderItem.ApilableVenta,
                    orderItem.TipoAlmacenamientoVenta,
                    orderItem.PrecioCompraUnitario,
                    orderItem.PrecioVentaUnitario,
                    orderItem.VariacionStock,
                    orderItem.StockMinimo,
                    orderItem.CreatedAt,
                    orderItem.UpdatedAt))
                .ToList());

    private static string GenerateImportOrderNumber()
        => $"WSU-IMP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

    private static (List<CreateWsuOrderItemInput> Items, List<object> RowErrors) ParseOrderItemsFromExcel(Stream stream, OrderType orderType)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null)
        {
            return (new List<CreateWsuOrderItemInput>(), new List<object> { new { row = 0, field = "Worksheet", message = "The workbook has no worksheets." } });
        }

        var headerRow = worksheet.FirstRowUsed();
        if (headerRow is null)
        {
            return (new List<CreateWsuOrderItemInput>(), new List<object> { new { row = 0, field = "Header", message = "The worksheet is empty." } });
        }

        var columns = headerRow.CellsUsed()
            .ToDictionary(cell => NormalizeHeader(cell.GetString()), cell => cell.Address.ColumnNumber);

        if (!columns.TryGetValue(NormalizeHeader("VariacionStock"), out var variacionStockColumn))
        {
            return (new List<CreateWsuOrderItemInput>(), new List<object> { new { row = 1, field = "VariacionStock", message = "Required column not found." } });
        }

        var items = new List<CreateWsuOrderItemInput>();
        var rowErrors = new List<object>();
        var firstRowByTriplet = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            if (row.CellsUsed().All(cell => string.IsNullOrWhiteSpace(cell.GetString())))
            {
                continue;
            }

            var rowNumber = row.RowNumber();
            if (!TryGetDecimal(row.Cell(variacionStockColumn), out var variacionStock))
            {
                rowErrors.Add(new { row = rowNumber, field = "VariacionStock", message = "Invalid decimal value." });
                continue;
            }

            var item = new CreateWsuOrderItemInput(
                GetGuid(columns, row, "ProductPublicId"),
                GetString(columns, row, "SkuWsu"),
                GetString(columns, row, "NombreSkuWsu"),
                GetString(columns, row, "SkuProveedor"),
                GetString(columns, row, "NombreSkuProveedor"),
                GetString(columns, row, "RutProveedor"),
                GetString(columns, row, "RsProveedor"),
                GetString(columns, row, "SkuCliente"),
                GetString(columns, row, "NombreSkuCliente"),
                GetString(columns, row, "RutCliente"),
                GetString(columns, row, "RsCliente"),
                GetString(columns, row, "Posicion"),
                GetString(columns, row, "UnidadDeMedida"),
                GetDecimal(columns, row, "LoteMinimoCompra"),
                GetDecimal(columns, row, "LargoCompraCm"),
                GetDecimal(columns, row, "AnchoCompraCm"),
                GetDecimal(columns, row, "AltoCompraCm"),
                GetDecimal(columns, row, "PesoCompraKg"),
                GetBool(columns, row, "ApilableCompra"),
                GetString(columns, row, "TipoAlmacenamientoCompra"),
                GetDecimal(columns, row, "LoteMinimoVenta"),
                GetDecimal(columns, row, "LargoVentaCm"),
                GetDecimal(columns, row, "AnchoVentaCm"),
                GetDecimal(columns, row, "AltoVentaCm"),
                GetDecimal(columns, row, "PesoVentaKg"),
                GetBool(columns, row, "ApilableVenta"),
                GetString(columns, row, "TipoAlmacenamientoVenta"),
                GetDecimal(columns, row, "PrecioCompraUnitario"),
                GetDecimal(columns, row, "PrecioVentaUnitario"),
                variacionStock,
                GetDecimal(columns, row, "StockMinimo"),
                GetString(columns, row, "Notas") ?? GetString(columns, row, "Notes"));

            var tripletKey = BuildSkuTripletKey(item.SkuWsu, item.SkuProveedor, item.SkuCliente);
            if (firstRowByTriplet.TryGetValue(tripletKey, out var firstRow))
            {
                rowErrors.Add(new
                {
                    row = rowNumber,
                    field = "SkuTriplet",
                    message = $"Duplicate SKU triplet detected (matches row {firstRow})."
                });
                continue;
            }

            firstRowByTriplet[tripletKey] = rowNumber;
            items.Add(item);
        }

        return (items, rowErrors);
    }

    private static (List<ImportWsuOrderEpcLineInput> Items, List<object> RowErrors) ParseOrderEpcsFromExcel(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheets.FirstOrDefault();
        if (worksheet is null)
        {
            return (new List<ImportWsuOrderEpcLineInput>(), new List<object> { new { row = 0, field = "Worksheet", message = "The workbook has no worksheets." } });
        }

        var headerRow = worksheet.FirstRowUsed();
        if (headerRow is null)
        {
            return (new List<ImportWsuOrderEpcLineInput>(), new List<object> { new { row = 0, field = "Header", message = "The worksheet is empty." } });
        }

        var columns = headerRow.CellsUsed()
            .ToDictionary(cell => NormalizeHeader(cell.GetString()), cell => cell.Address.ColumnNumber);

        if (!columns.TryGetValue(NormalizeHeader("Epc"), out var epcColumn))
        {
            return (new List<ImportWsuOrderEpcLineInput>(), new List<object> { new { row = 1, field = "Epc", message = "Required column not found." } });
        }

        var items = new List<ImportWsuOrderEpcLineInput>();
        var rowErrors = new List<object>();
        var firstRowByEpc = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            if (row.CellsUsed().All(cell => string.IsNullOrWhiteSpace(cell.GetString())))
            {
                continue;
            }

            var rowNumber = row.RowNumber();
            var epc = row.Cell(epcColumn).GetString().Trim();
            if (string.IsNullOrWhiteSpace(epc))
            {
                rowErrors.Add(new { row = rowNumber, field = "Epc", message = "Epc is required." });
                continue;
            }

            if (firstRowByEpc.TryGetValue(epc, out var firstRow))
            {
                rowErrors.Add(new
                {
                    row = rowNumber,
                    field = "Epc",
                    message = $"Duplicate EPC detected (matches row {firstRow})."
                });
                continue;
            }

            firstRowByEpc[epc] = rowNumber;
            items.Add(new ImportWsuOrderEpcLineInput(rowNumber, epc));
        }

        return (items, rowErrors);
    }

    private static string[] GetTemplateHeaders()
        =>
        [
            "SkuWsu", "NombreSkuWsu", "SkuProveedor", "NombreSkuProveedor", "RutProveedor", "RsProveedor",
            "SkuCliente", "NombreSkuCliente", "RutCliente", "RsCliente", "Posicion",
            "UnidadDeMedida", "LoteMinimoCompra", "LargoCompraCm", "AnchoCompraCm", "AltoCompraCm", "PesoCompraKg",
            "ApilableCompra", "TipoAlmacenamientoCompra", "LoteMinimoVenta", "LargoVentaCm", "AnchoVentaCm", "AltoVentaCm",
            "PesoVentaKg", "ApilableVenta", "TipoAlmacenamientoVenta", "PrecioCompraUnitario", "PrecioVentaUnitario",
            "VariacionStock", "StockMinimo"
        ];

    private static string? GetString(IReadOnlyDictionary<string, int> columns, IXLRow row, string header)
    {
        if (!TryGetColumn(columns, header, out var column)) return null;
        var value = row.Cell(column).GetString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static Guid? GetGuid(IReadOnlyDictionary<string, int> columns, IXLRow row, string header)
    {
        var raw = GetString(columns, row, header);
        return Guid.TryParse(raw, out var value) ? value : null;
    }

    private static decimal? GetDecimal(IReadOnlyDictionary<string, int> columns, IXLRow row, string header)
    {
        if (!TryGetColumn(columns, header, out var column)) return null;
        var cell = row.Cell(column);
        return TryGetDecimal(cell, out var value) ? value : null;
    }

    private static bool? GetBool(IReadOnlyDictionary<string, int> columns, IXLRow row, string header)
    {
        if (!TryGetColumn(columns, header, out var column)) return null;
        var text = row.Cell(column).GetString().Trim();
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (bool.TryParse(text, out var boolValue)) return boolValue;
        if (text == "1" || text.Equals("si", StringComparison.OrdinalIgnoreCase) || text.Equals("sí", StringComparison.OrdinalIgnoreCase)) return true;
        if (text == "0" || text.Equals("no", StringComparison.OrdinalIgnoreCase)) return false;
        return null;
    }

    private static bool TryGetDecimal(IXLCell cell, out decimal value)
    {
        if (cell.TryGetValue<decimal>(out value)) return true;
        var text = cell.GetString().Trim();
        if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value)) return true;
        if (decimal.TryParse(text, NumberStyles.Any, new CultureInfo("es-CL"), out value)) return true;
        value = 0m;
        return false;
    }

    private static bool TryGetColumn(IReadOnlyDictionary<string, int> columns, string header, out int column)
        => columns.TryGetValue(NormalizeHeader(header), out column);

    private static string BuildSkuTripletKey(string? skuWsu, string? skuProveedor, string? skuCliente)
        => string.Join("|", NormalizeSkuValue(skuWsu), NormalizeSkuValue(skuProveedor), NormalizeSkuValue(skuCliente));

    private static string NormalizeSkuValue(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    private static string NormalizeHeader(string header)
        => new string(header
            .Trim()
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());
}
