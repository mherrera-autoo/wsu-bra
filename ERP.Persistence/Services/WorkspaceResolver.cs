using ERP.Modules.Identity.Application.Services;
using ERP.Modules.MasterData.Contracts;
using ERP.Persistence.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ERP.Persistence.Services;

public sealed class WorkspaceResolver : IWorkspaceResolver
{
    private static readonly OrganizationType[] PreferredOrder =
    {
        OrganizationType.MultiCompany,
        OrganizationType.Holding,
        OrganizationType.Individual
    };

    private readonly ErpDbContext _dbContext;
    private readonly ILogger<WorkspaceResolver> _logger;

    public WorkspaceResolver(ErpDbContext dbContext, ILogger<WorkspaceResolver> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<long> ResolveWorkspaceOrganizationIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            throw new WorkspaceResolutionException("Workspace resolution failed because the user identifier is invalid.");
        }

        var organizations = await _dbContext.OrganizationMembers
            .AsNoTracking()
            .Where(member => member.UserId == userId)
            .Join(
                _dbContext.Organizations.AsNoTracking(),
                member => member.OrganizationId,
                organization => organization.Id,
                (member, organization) => new WorkspaceCandidate(
                    organization.Id,
                    organization.AccountType,
                    organization.CreatedAt))
            .ToListAsync(cancellationToken);

        if (organizations.Count == 0)
        {
            throw new WorkspaceResolutionException("Workspace resolution failed because the user is not a member of any organization.");
        }

        var orderedCandidates = organizations
            .OrderBy(candidate => candidate.CreatedAt)
            .ThenBy(candidate => candidate.OrganizationId)
            .ToList();

        var preferredType = PreferredOrder.FirstOrDefault(type => orderedCandidates.Any(candidate => candidate.AccountType == type));
        if (preferredType == default)
        {
            throw new WorkspaceResolutionException("Workspace resolution failed because no supported organizations were found.");
        }

        var preferredCandidates = orderedCandidates
            .Where(candidate => candidate.AccountType == preferredType)
            .ToList();

        if (preferredCandidates.Count > 1)
        {
            _logger.LogWarning(
                "Multiple workspace organizations of type {AccountType} found for user {UserId}. Selecting {OrganizationId}.",
                preferredType,
                userId,
                preferredCandidates[0].OrganizationId);
        }

        return preferredCandidates[0].OrganizationId;
    }

    public Task<bool> IsOrganizationMemberAsync(long userId, long organizationId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || organizationId <= 0)
        {
            return Task.FromResult(false);
        }

        return _dbContext.OrganizationMembers
            .AsNoTracking()
            .AnyAsync(member => member.UserId == userId && member.OrganizationId == organizationId, cancellationToken);
    }

    private sealed record WorkspaceCandidate(long OrganizationId, OrganizationType AccountType, DateTime CreatedAt);
}
