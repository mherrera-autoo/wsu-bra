using ERP.Modules.Identity.Application.Repositories;
using ERP.Modules.Identity.Contracts;
using ERP.Modules.MasterData.Contracts;
using System;
using System.Linq;
using IdentityTaxEntityRepository = ERP.Modules.Identity.Application.Repositories.ITaxEntityRepository;

namespace ERP.Modules.Identity.Application.Services;

public sealed class TaxEntityService
{
    private readonly ICompanyLookupRepository _companyLookupRepository;
    private readonly IdentityTaxEntityRepository _taxEntityRepository;

    public TaxEntityService(
        ICompanyLookupRepository companyLookupRepository,
        IdentityTaxEntityRepository taxEntityRepository)
    {
        _companyLookupRepository = companyLookupRepository;
        _taxEntityRepository = taxEntityRepository;
    }

public async Task<IReadOnlyList<TaxEntitySnapshot>> ListForCompanyAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var companyPublicId = await _companyLookupRepository.GetCompanyPublicIdAsync(companyId, cancellationToken);
        if (!companyPublicId.HasValue)
        {
            return Array.Empty<TaxEntitySnapshot>();
        }

        return await _taxEntityRepository.ListByCompanyPublicIdAsync(companyPublicId.Value, cancellationToken);
    }

    public async Task<CompanyTaxEntityInfo?> GetCompanyTaxEntityAsync(long companyId, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return null;
        }

        var entries = await _taxEntityRepository.ListByCompanyIdsAsync(new[] { companyId }, cancellationToken);
        return entries.FirstOrDefault();
    }

    public async Task<IReadOnlyList<TaxEntityAccessSummary>> ListAccessibleAsync(
        IReadOnlyList<CompanyAccessSummary> companies,
        CancellationToken cancellationToken = default)
    {
        if (companies.Count == 0)
        {
            return Array.Empty<TaxEntityAccessSummary>();
        }

        var companyIds = companies.Select(company => company.CompanyId).Distinct().ToArray();
        var taxEntities = await _taxEntityRepository.ListByCompanyIdsAsync(companyIds, cancellationToken);

        if (taxEntities.Count == 0)
        {
            return Array.Empty<TaxEntityAccessSummary>();
        }

        var companyLookup = companies.ToDictionary(company => company.CompanyId);
        var grouped = taxEntities
            .Where(entry => companyLookup.ContainsKey(entry.CompanyId))
            .GroupBy(entry => new
            {
                entry.TaxEntityId,
                entry.TaxEntityPublicId,
                entry.TaxId,
                entry.DisplayName
            });

        var summaries = grouped.Select(group =>
        {
            var hasOperableCompany = false;
            var companyIds = new HashSet<long>();
            var sourceFlags = CompanyAccessSource.None;

            Guid companyPublicId = Guid.Empty;
            CompanyLinkAccessType? maxAccessType = null;

            foreach (var entry in group)
            {
                var company = companyLookup[entry.CompanyId];

                if (company.HasAccess)
                    hasOperableCompany = true;

                companyIds.Add(company.CompanyId);
                sourceFlags |= company.SourceFlags;

                if (company.AccessType.HasValue)
                {
                    var current = company.AccessType.Value;
                    if (!maxAccessType.HasValue || current > maxAccessType.Value)
                    {
                        maxAccessType = current;
                        companyPublicId = company.CompanyPublicId;
                    }
                }
            }

            return new TaxEntityAccessSummary(
                group.Key.TaxEntityPublicId,
                group.Key.TaxEntityId,
                group.Key.TaxId,
                group.Key.DisplayName,
                hasOperableCompany,
                companyIds.Count,
                sourceFlags,
                maxAccessType,
                companyPublicId
            );
        }).ToList();

        return summaries
            .OrderBy(summary => summary.DisplayName ?? summary.TaxId, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
