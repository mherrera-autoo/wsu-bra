using ERP.Api.Authorization;
using ERP.Api.Contracts.Companies;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.MasterData.Contracts;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace ERP.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ICompanyAccessRepository _companyAccessRepository;
    private readonly ICompanyRepository _companyRepository;

    public CompaniesController(
        ICurrentUserProvider currentUserProvider,
        ICompanyAccessRepository companyAccessRepository,
        ICompanyRepository companyRepository)
    {
        _currentUserProvider = currentUserProvider;
        _companyAccessRepository = companyAccessRepository;
        _companyRepository = companyRepository;
    }

    [HttpGet]
    [RequirePlatformPermission(PermissionKeys.Platform.GlobalIAMManager)]
    public async Task<IActionResult> ListAllCompanies(CancellationToken cancellationToken)
    {
        if (!TryGetUser(out var currentUser))
        {
            return Unauthorized();
        }

        var companies = await _companyRepository.ListAllAsync(cancellationToken);
        var companyAccess = await _companyAccessRepository.ListByUserAsync(currentUser.UserId, cancellationToken);
        var assignedCompanyPublicIds = companyAccess
            .Select(access => access.CompanyPublicId)
            .ToHashSet();

        var response = companies
            .Select(company => new CompanyWithAssignmentSummary(
                company.Id,
                company.PublicId,
                company.OrganizationId,
                company.TaxEntityId,
                company.Name,
                assignedCompanyPublicIds.Contains(company.PublicId)))
            .ToList();

        return Ok(response);
    }

    private bool TryGetUser([NotNullWhen(true)] out CurrentUser? currentUser)
    {
        currentUser = _currentUserProvider.GetCurrentUser();
        if (currentUser is null)
        {
            return false;
        }

        return true;
    }
}
