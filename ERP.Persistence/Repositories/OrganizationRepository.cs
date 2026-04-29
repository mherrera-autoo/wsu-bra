using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using ApplicationOrganizationRepository = ERP.Modules.MasterData.Application.Repositories.IOrganizationRepository;
using ContractOrganizationRepository = ERP.Modules.MasterData.Contracts.IOrganizationRepository;

namespace ERP.Persistence.Repositories;

public sealed class OrganizationRepository : ContractOrganizationRepository, ApplicationOrganizationRepository
{
    private readonly ErpDbContext _dbContext;

    public OrganizationRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<OrganizationSnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Organizations
            .AsNoTracking()
            .Where(org => org.Id == id)
            .Select(org => new OrganizationSnapshot(
                org.Id,
                org.PublicId,
                org.AccountType,
                org.DisplayName))
            .FirstOrDefaultAsync(cancellationToken);

    public Task<OrganizationSnapshot?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
        => _dbContext.Organizations
            .AsNoTracking()
            .Where(org => org.PublicId == publicId)
            .Select(org => new OrganizationSnapshot(
                org.Id,
                org.PublicId,
                org.AccountType,
                org.DisplayName))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<OrganizationSnapshot> AddAsync(OrganizationCreateRequest organization, CancellationToken cancellationToken = default)
    {
        var entity = Organization.Create(organization.AccountType, organization.DisplayName);
        await _dbContext.Organizations.AddAsync(entity, cancellationToken);
        return new OrganizationSnapshot(
            entity.Id,
            entity.PublicId,
            entity.AccountType,
            entity.DisplayName);
    }
}
