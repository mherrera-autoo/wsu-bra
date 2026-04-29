using ERP.Api.Authorization;
using ERP.Api.Contracts.Inventory;
using ERP.Modules.Inventory.Application.Repositories;
using ERP.Modules.Inventory.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/inventory")]
public sealed class InventoryController : ControllerBase
{
    private readonly IStockRepository _stockRepository;
    private readonly IInventoryFlowService _inventoryFlowService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;

    public InventoryController(
        IStockRepository stockRepository,
        IInventoryFlowService inventoryFlowService,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider)
    {
        _stockRepository = stockRepository;
        _inventoryFlowService = inventoryFlowService;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
    }

    [HttpGet("principles")]
    public IActionResult Principles()
    {
        return Ok(new
        {
            inventory = "products (structure)",
            stock = "quantities per warehouse (reality)",
            pricing = "sales strategy (business)",
            rule = "stock is the result of movements, not manual updates"
        });
    }

[HttpGet("stock")]
    [RequireCompanyPermission(PermissionKeys.Inventory.StockRead)]
    public async Task<IActionResult> GetStock(
        [FromQuery] long productId,
        [FromQuery] long warehouseId,
        CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var stock = await _stockRepository.GetAsync(companyId, productId, warehouseId, cancellationToken);
        return Ok(new
        {
            companyId,
            productId,
            warehouseId,
            onHandQuantity = stock?.OnHandQuantity ?? 0m
        });
    }

    [HttpPost("transfer")]
    [RequireCompanyPermission(PermissionKeys.Inventory.MovementsCreate)]
    public async Task<IActionResult> TransferStock(TransferStockRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(token =>
            _inventoryFlowService.TransferStockAsync(
                companyId,
                request.ProductId,
                request.FromWarehouseId,
                request.ToWarehouseId,
                request.Quantity,
                request.ReferenceType,
                request.ReferenceId,
                request.Reason,
                token),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok" });
    }

    [HttpPost("adjustments")]
    [RequireCompanyPermission(PermissionKeys.Inventory.AdjustmentsCreate)]
    public async Task<IActionResult> AdjustStock(AdjustStockRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(token =>
            _inventoryFlowService.AdjustStockAsync(
                companyId,
                request.ProductId,
                request.WarehouseId,
                request.Quantity,
                request.Increase,
                request.ReferenceType,
                request.ReferenceId,
                request.Reason,
                token),
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "ok" });
    }

    private bool TryGetCompanyId(out long companyId)
    {
        var currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            companyId = default;
            return false;
        }

        companyId = currentUser.CompanyId;
        return true;
    }
}
