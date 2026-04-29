using ERP.Api.Authorization;
using ERP.Api.Contracts.MasterData;
using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[Tags("Platform")]
[ApiController]
[Authorize]
[RequirePlatformPermission(PermissionKeys.Platform.WsuManage)]
[Route("api/platform/companies")]
public sealed class PlatformCompaniesController : ControllerBase
{
    private readonly ICompanyRepository _companyRepository;
    private readonly MasterDataService _masterDataService;

    public PlatformCompaniesController(
        ICompanyRepository companyRepository,
        MasterDataService masterDataService)
    {
        _companyRepository = companyRepository;
        _masterDataService = masterDataService;
    }

    [HttpGet("{companyPublicId:guid}/warehouses")]
    public async Task<ActionResult<IReadOnlyList<WarehouseSummary>>> ListWarehouses(
        Guid companyPublicId,
        CancellationToken cancellationToken)
    {
        if (companyPublicId == Guid.Empty)
        {
            return BadRequest(new { error = "companyPublicId is required." });
        }

        var companyId = await _companyRepository.GetIdByPublicIdAsync(companyPublicId, cancellationToken);
        if (!companyId.HasValue)
        {
            return NotFound(new { error = "Company not found." });
        }

        var warehouses = await _masterDataService.ListWarehousesAsync(companyId.Value, cancellationToken);
        var response = warehouses.Select(warehouse => new WarehouseSummary(
            warehouse.Id,
            warehouse.PublicId,
            warehouse.Code,
            warehouse.Name,
            warehouse.Location));

        return Ok(response);
    }
}
