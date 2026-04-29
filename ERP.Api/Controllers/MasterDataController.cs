using ERP.Api.Authorization;
using ERP.Api.Contracts.MasterData;
using ERP.Modules.MasterData.Application.Services;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[TenantGuard]
[Route("api/masterdata")]
public sealed class MasterDataController : ControllerBase
{
    private readonly MasterDataService _masterDataService;
    private readonly ICurrentUserProvider _currentUserProvider;

    public MasterDataController(MasterDataService masterDataService, ICurrentUserProvider currentUserProvider)
    {
        _masterDataService = masterDataService;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("/api/companies/products")]
    public async Task<IActionResult> CreateProduct(CreateProductRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _masterDataService.CreateProductAsync(
            companyId,
            request.Sku,
            request.Name,
            request.Barcode,
            request.UnitOfMeasureId,
            request.IsStockable,
            request.IsSellable,
            request.IsPurchasable,
            cancellationToken: cancellationToken,
            isStackable: request.IsStackable,
            lengthCm: request.LengthCm,
            widthCm: request.WidthCm,
            weightKg: request.WeightKg,
            storageType: request.StorageType);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpGet("/api/companies/products")]
    public async Task<ActionResult<IReadOnlyList<ProductSummary>>> ListProducts(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var products = await _masterDataService.ListProductsAsync(companyId, cancellationToken);
        var response = products.Select(product => new ProductSummary(
            product.Id,
            product.Sku,
            product.Name,
            product.Barcode,
            product.IsStockable,
            product.IsSellable,
            product.IsPurchasable,
            product.IsStackable,
            product.LengthCm,
            product.WidthCm,
            product.WeightKg,
            product.StorageType,
            product.UnitOfMeasure?.DisplayCode ?? string.Empty,
            product.UnitOfMeasure?.Translations.FirstOrDefault(translation => !string.IsNullOrWhiteSpace(translation.Symbol))?.Symbol ?? string.Empty));
        return Ok(response);
    }

    [HttpGet("/api/companies/products/{id:long}")]
    public async Task<ActionResult<ProductSummary>> GetProduct(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var product = await _masterDataService.GetProductAsync(companyId, id, cancellationToken);
        if (product is null)
        {
            return NotFound();
        }

        return Ok(new ProductSummary(
            product.Id,
            product.Sku,
            product.Name,
            product.Barcode,
            product.IsStockable,
            product.IsSellable,
            product.IsPurchasable,
            product.IsStackable,
            product.LengthCm,
            product.WidthCm,
            product.WeightKg,
            product.StorageType,
            product.UnitOfMeasure?.DisplayCode ?? string.Empty,
            product.UnitOfMeasure?.Translations.FirstOrDefault(translation => !string.IsNullOrWhiteSpace(translation.Symbol))?.Symbol ?? string.Empty));
    }

    [HttpPost("/api/companies/customers")]
    public async Task<IActionResult> CreateCustomer(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _masterDataService.CreateCustomerAsync(
            companyId,
            request.Name,
            request.TaxId,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(ToCustomerSummary(result.Value!));
    }

    [HttpGet("/api/companies/customers")]
    public async Task<ActionResult<IReadOnlyList<CustomerSummary>>> ListCustomers(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var customers = await _masterDataService.ListCustomersAsync(companyId, cancellationToken);
        var response = customers.Select(ToCustomerSummary);
        return Ok(response);
    }

    [HttpGet("/api/companies/customers/{id:long}")]
    public async Task<ActionResult<CustomerSummary>> GetCustomer(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var customer = await _masterDataService.GetCustomerAsync(companyId, id, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        return Ok(ToCustomerSummary(customer));
    }

    [HttpPut("/api/companies/customers/{id:long}")]
    public async Task<IActionResult> UpdateCustomer(long id, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        if (request.CompanyId != companyId)
        {
            return Forbid();
        }

        var result = await _masterDataService.UpdateCustomerAsync(
            companyId,
            id,
            request.Name,
            request.TaxId,
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Customer not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(ToCustomerSummary(result.Value!));
    }

    [HttpDelete("/api/companies/customers/{id:long}")]
    public async Task<IActionResult> DeleteCustomer(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _masterDataService.DeleteCustomerAsync(companyId, id, cancellationToken);
        if (!result.Success)
        {
            if (string.Equals(result.Error, "Customer not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "deleted" });
    }

    [HttpPost("warehouses")]
    public async Task<IActionResult> CreateWarehouse(CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _masterDataService.CreateWarehouseAsync(
            companyId,
            request.Code,
            request.Name,
            cancellationToken: cancellationToken,
            location: request.Location);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Warehouse not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpGet("warehouses")]
    public async Task<ActionResult<IReadOnlyList<WarehouseSummary>>> ListWarehouses(CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var warehouses = await _masterDataService.ListWarehousesAsync(companyId, cancellationToken);
        var response = warehouses.Select(warehouse => new WarehouseSummary(
            warehouse.Id,
            warehouse.PublicId,
            warehouse.Code,
            warehouse.Name,
            warehouse.Location));
        return Ok(response);
    }

    [HttpGet("warehouses/{id:long}")]
    public async Task<ActionResult<WarehouseSummary>> GetWarehouse(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var warehouse = await _masterDataService.GetWarehouseAsync(companyId, id, cancellationToken);
        if (warehouse is null)
        {
            return NotFound();
        }

        return Ok(new WarehouseSummary(warehouse.Id, warehouse.PublicId, warehouse.Code, warehouse.Name, warehouse.Location));
    }

    [HttpPut("warehouses/{id:long}")]
    public async Task<IActionResult> UpdateWarehouse(long id, UpdateWarehouseRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _masterDataService.UpdateWarehouseAsync(
            companyId,
            id,
            request.Code,
            request.Name,
            request.IsActive,
            cancellationToken: cancellationToken,
            location: request.Location);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "Warehouse not found.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { error = result.Error });
            }

            return BadRequest(new { error = result.Error });
        }

        return Ok(new { result.Value!.Id });
    }

    [HttpDelete("warehouses/{id:long}")]
    public async Task<IActionResult> DeleteWarehouse(long id, CancellationToken cancellationToken)
    {
        if (!TryGetCompanyId(out var companyId))
        {
            return Unauthorized();
        }

        var result = await _masterDataService.DeleteWarehouseAsync(companyId, id, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(new { error = result.Error });
        }

        return Ok(new { status = "deleted" });
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

    private static CustomerSummary ToCustomerSummary(ERP.Modules.MasterData.Domain.Customer customer)
        => new(customer.Id, customer.Name, customer.TaxId);
}
