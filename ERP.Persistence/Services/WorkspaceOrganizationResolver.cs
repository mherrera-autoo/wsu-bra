using ERP.Modules.Identity.Application.Services;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.Identity.Domain;
using ERP.Persistence.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Persistence.Services;

public sealed class WorkspaceOrganizationResolver : IWorkspaceOrganizationResolver
{
    private readonly ErpDbContext _dbContext;
    private readonly ILogger<WorkspaceOrganizationResolver> _logger;

    public WorkspaceOrganizationResolver(ErpDbContext dbContext, ILogger<WorkspaceOrganizationResolver> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<long?> ResolveWorkspaceOrganizationIdAsync(long userId, long? organizationIdFromJwt, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            _logger.LogWarning("Invalid userId provided: {UserId}", userId);
            return null;
        }

        // If JWT has organization, check if it's MultiCompany first
        if (organizationIdFromJwt.HasValue && organizationIdFromJwt.Value > 0)
        {
            var jwtOrganization = await _dbContext.Organizations
                .AsNoTracking()
                .Where(org => org.Id == organizationIdFromJwt.Value)
                .Select(org => new { org.Id, org.AccountType })
                .FirstOrDefaultAsync(cancellationToken);

            if (jwtOrganization != null && jwtOrganization.AccountType == OrganizationType.MultiCompany)
            {
                return jwtOrganization.Id;
            }
        }

        // Search user's MultiCompany organizations with deterministic Owner-first ordering
        var workspaceOrganization = await _dbContext.OrganizationMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .Join(
                _dbContext.Organizations.AsNoTracking(),
                member => member.OrganizationId,
                organization => organization.Id,
                (member, organization) => new 
                { 
                    OrganizationId = organization.Id,
                    AccountType = organization.AccountType,
                    Role = member.Role
                })
            .Where(x => x.AccountType == OrganizationType.MultiCompany)
            .OrderBy(x => x.Role == OrganizationRole.Owner ? 0 : 1) // Owner first
            .ThenBy(x => x.OrganizationId) // Deterministic tie-breaker
            .Select(x => x.OrganizationId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workspaceOrganization > 0)
        {
            return workspaceOrganization;
        }

        _logger.LogInformation("No MultiCompany workspace found for user {UserId}", userId);
        return null;
    }
}