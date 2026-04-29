using ERP.Modules.Identity.Contracts;
using ERP.Modules.Identity.Application.Services;
using ERP.Modules.Identity.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Persistence.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Services;

public sealed class CompanyAccessResolver : ICompanyAccessResolver
{
    private readonly ErpDbContext _dbContext;
    private readonly IReadOnlyCollection<ICompanyAccessSource> _sources;

    public CompanyAccessResolver(
        ErpDbContext dbContext,
        IEnumerable<ICompanyAccessSource> sources)
    {
        _dbContext = dbContext;
        _sources = sources.ToArray();
    }

    public async Task<IReadOnlyList<CompanyAccessSummary>> ResolveAsync(
        long userId,
        long organizationId,
        CompanyListScope scope,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0 || organizationId <= 0)
        {
            return Array.Empty<CompanyAccessSummary>();
        }

        var organization = await _dbContext.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == organizationId, cancellationToken);

        if (organization is null)
        {
            return Array.Empty<CompanyAccessSummary>();
        }

        var candidates = new List<CompanyAccessCandidate>();
        foreach (var source in _sources.Where(source => source.Supports(organization.AccountType)))
        {
            var sourceCandidates = await source.GetCandidatesAsync(userId, organizationId, cancellationToken);
            candidates.AddRange(sourceCandidates);
        }

        if (candidates.Count == 0)
        {
            return Array.Empty<CompanyAccessSummary>();
        }

        var aggregateByCompany = new Dictionary<Guid, CompanyAccessAggregate>();
        foreach (var candidate in candidates)
        {
            if (!aggregateByCompany.TryGetValue(candidate.CompanyPublicId, out var aggregate))
            {
                aggregate = new CompanyAccessAggregate(candidate.CompanyPublicId);
                aggregateByCompany[candidate.CompanyPublicId] = aggregate;
            }

            aggregate.SourceFlags |= candidate.Source;
            if (candidate.AccessType is not null)
            {
                aggregate.AccessType ??= candidate.AccessType;
            }
        }

        var companyPublicIds = aggregateByCompany.Keys.ToArray();

        var companies = await _dbContext.Companies
            .AsNoTracking()
            .Where(company => companyPublicIds.Contains(company.PublicId))
            .Select(company => new
            {
                company.PublicId,
                company.Id,
                company.OrganizationId,
                company.Name
            })
            .ToListAsync(cancellationToken);

        if (companies.Count == 0)
        {
            return Array.Empty<CompanyAccessSummary>();
        }

        var activeCompanyPublicIds = await _dbContext.CompanyUsers
            .AsNoTracking()
            .Where(companyUser => companyUser.UserId == userId
                && companyUser.Status == CompanyUserStatus.Active
                && companyPublicIds.Contains(companyUser.CompanyPublicId))
            .Select(companyUser => companyUser.CompanyPublicId)
            .ToListAsync(cancellationToken);

        var activeCompanySet = activeCompanyPublicIds.ToHashSet();
        var summaries = new List<CompanyAccessSummary>();

        foreach (var company in companies)
        {
            if (!aggregateByCompany.TryGetValue(company.PublicId, out var aggregate))
            {
                continue;
            }

            var hasAccess = activeCompanySet.Contains(company.PublicId);
            if (scope == CompanyListScope.OperableOnly && !hasAccess)
            {
                continue;
            }

            summaries.Add(new CompanyAccessSummary(
                company.PublicId,
                company.Id,
                company.OrganizationId,
                company.Name,
                hasAccess,
                aggregate.SourceFlags,
                aggregate.AccessType));
        }

        return summaries
            .OrderBy(summary => summary.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private sealed class CompanyAccessAggregate
    {
        public CompanyAccessAggregate(Guid companyPublicId)
        {
            CompanyPublicId = companyPublicId;
        }

        public Guid CompanyPublicId { get; }
        public CompanyAccessSource SourceFlags { get; set; }
        public CompanyLinkAccessType? AccessType { get; set; }
    }
}
