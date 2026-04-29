using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PharmaProductProfileRepository : IPharmaProductProfileRepository
{
    private readonly ErpDbContext _dbContext;

    public PharmaProductProfileRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PharmaProductProfile?> GetByProductAsync(long companyId, long productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PharmaProductProfiles
            .FirstOrDefaultAsync(profile => profile.CompanyId == companyId && profile.ProductId == productId, cancellationToken);
    }

    public async Task<IReadOnlyList<PharmaProductProfile>> GetByProductsAsync(long companyId, IEnumerable<long> productIds, CancellationToken cancellationToken = default)
    {
        var ids = productIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return Array.Empty<PharmaProductProfile>();
        }

        return await _dbContext.PharmaProductProfiles
            .Where(profile => profile.CompanyId == companyId && ids.Contains(profile.ProductId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(PharmaProductProfile profile, CancellationToken cancellationToken = default)
    {
        await _dbContext.PharmaProductProfiles.AddAsync(profile, cancellationToken);
    }
}
