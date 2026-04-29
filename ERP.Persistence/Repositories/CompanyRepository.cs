using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using ContractCompanyRepository = ERP.Modules.MasterData.Contracts.ICompanyRepository;
using ApplicationCompanyRepository = ERP.Modules.MasterData.Application.Repositories.ICompanyRepository;
using IdentityCompanyLookupRepository = ERP.Modules.Identity.Application.Repositories.ICompanyLookupRepository;

namespace ERP.Persistence.Repositories;

public sealed class CompanyRepository : ContractCompanyRepository, ApplicationCompanyRepository, IdentityCompanyLookupRepository
{
    private readonly ErpDbContext _dbContext;

    public CompanyRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<CompanySnapshot>> ListAllAsync(CancellationToken cancellationToken = default)
        => await _dbContext.Companies
            .AsNoTracking()
            .OrderBy(company => company.Name)
            .Select(company => new CompanySnapshot(
                company.Id,
                company.PublicId,
                company.OrganizationId,
                company.TaxEntityId,
                company.Name))
            .ToListAsync(cancellationToken);

    public Task<CompanySnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Companies
            .AsNoTracking()
            .Where(company => company.Id == id)
            .Select(company => new CompanySnapshot(
                company.Id,
                company.PublicId,
                company.OrganizationId,
                company.TaxEntityId,
                company.Name))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<CompanySnapshot?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
        => _dbContext.Companies
            .AsNoTracking()
            .Where(company => company.PublicId == publicId)
            .Select(company => new CompanySnapshot(
                company.Id,
                company.PublicId,
                company.OrganizationId,
                company.TaxEntityId,
                company.Name))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<long?> GetIdByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
        => await _dbContext.Companies
            .Where(company => company.PublicId == publicId)
            .Select(company => (long?)company.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<CompanySnapshot?> GetByTaxEntityIdAsync(long taxEntityId, CancellationToken cancellationToken = default)
        => _dbContext.Companies
            .AsNoTracking()
            .Where(company => company.TaxEntityId == taxEntityId)
            .Select(company => new CompanySnapshot(
                company.Id,
                company.PublicId,
                company.OrganizationId,
                company.TaxEntityId,
                company.Name))
            .FirstOrDefaultAsync(cancellationToken);

public async Task<long?> GetOrganizationIdAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.Companies
            .Where(company => company.Id == companyId)
            .Select(company => (long?)company.OrganizationId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Guid?> GetCompanyPublicIdAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.Companies
            .Where(company => company.Id == companyId)
            .Select(company => (Guid?)company.PublicId)
            .FirstOrDefaultAsync(cancellationToken);

public async Task<CompanySnapshot> AddAsync(CompanyCreateRequest company, CancellationToken cancellationToken = default)
    {
        var entity = Company.Create(company.OrganizationId, company.TaxEntityId, company.Name);
        await _dbContext.Companies.AddAsync(entity, cancellationToken);
        return new CompanySnapshot(
            entity.Id,
            entity.PublicId,
            entity.OrganizationId,
            entity.TaxEntityId,
            entity.Name);
    }

    public async Task UpdateTaxEntityIdAsync(long companyId, long taxEntityId, CancellationToken cancellationToken = default)
    {
        var company = await _dbContext.Companies
            .FirstOrDefaultAsync(c => c.Id == companyId, cancellationToken);
        if (company != null)
        {
            company.UpdateTaxEntityId(taxEntityId);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
