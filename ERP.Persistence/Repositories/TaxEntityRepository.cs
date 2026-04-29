using ERP.Modules.Identity.Contracts;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using IdentityTaxEntityRepository = ERP.Modules.Identity.Application.Repositories.ITaxEntityRepository;

namespace ERP.Persistence.Repositories;

public sealed class TaxEntityRepository : IdentityTaxEntityRepository
{
    private readonly ErpDbContext _dbContext;

    public TaxEntityRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

public Task<TaxEntitySnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.TaxEntities
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(entity => new TaxEntitySnapshot(
                entity.Id,
                entity.PublicId,
                entity.CompanyPublicId,
                entity.TaxId,
                entity.DisplayName))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<TaxEntitySnapshot?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
        => _dbContext.TaxEntities
            .AsNoTracking()
            .Where(entity => entity.PublicId == publicId)
            .Select(entity => new TaxEntitySnapshot(
                entity.Id,
                entity.PublicId,
                entity.CompanyPublicId,
                entity.TaxId,
                entity.DisplayName))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<TaxEntitySnapshot?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default)
    {
        var normalized = taxId.Trim();
        return _dbContext.TaxEntities
            .AsNoTracking()
            .Where(entity => entity.TaxId == normalized)
            .Select(entity => new TaxEntitySnapshot(
                entity.Id,
                entity.PublicId,
                entity.CompanyPublicId,
                entity.TaxId,
                entity.DisplayName))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TaxEntitySnapshot>> ListByCompanyPublicIdAsync(Guid companyPublicId, CancellationToken cancellationToken = default)
        => await _dbContext.TaxEntities
            .AsNoTracking()
            .Where(entity => entity.CompanyPublicId == companyPublicId)
            .OrderBy(entity => entity.Id)
            .Select(entity => new TaxEntitySnapshot(
                entity.Id,
                entity.PublicId,
                entity.CompanyPublicId,
                entity.TaxId,
                entity.DisplayName))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CompanyTaxEntityInfo>> ListByCompanyIdsAsync(IReadOnlyCollection<long> companyIds, CancellationToken cancellationToken = default)
    {
        if (companyIds.Count == 0)
        {
            return Array.Empty<CompanyTaxEntityInfo>();
        }

        return await _dbContext.Companies
            .AsNoTracking()
            .Where(company => companyIds.Contains(company.Id))
            .Join(
                _dbContext.TaxEntities.AsNoTracking(),
                company => new { company.PublicId, company.TaxEntityId },
                taxEntity => new { PublicId = taxEntity.CompanyPublicId, TaxEntityId = taxEntity.Id },
                (company, taxEntity) => new CompanyTaxEntityInfo(
                    company.Id,
                    taxEntity.Id,
                    taxEntity.PublicId,
                    taxEntity.TaxId,
                    taxEntity.DisplayName))
            .ToListAsync(cancellationToken);
    }

    public async Task<TaxEntitySnapshot> AddAsync(TaxEntityCreateRequest taxEntity, CancellationToken cancellationToken = default)
    {
        var entity = TaxEntity.Create(taxEntity.CompanyPublicId, taxEntity.TaxId, taxEntity.DisplayName);
        await _dbContext.TaxEntities.AddAsync(entity, cancellationToken);
        return new TaxEntitySnapshot(
            entity.Id,
            entity.PublicId,
            entity.CompanyPublicId,
            entity.TaxId,
            entity.DisplayName);
    }
}
