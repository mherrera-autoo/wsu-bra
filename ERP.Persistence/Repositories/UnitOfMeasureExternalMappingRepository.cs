using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class UnitOfMeasureExternalMappingRepository : IUnitOfMeasureExternalMappingRepository
{
    private readonly ErpDbContext _dbContext;

    public UnitOfMeasureExternalMappingRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UnitOfMeasureExternalMapping mapping, CancellationToken cancellationToken = default)
    {
        await _dbContext.UnitOfMeasureExternalMappings.AddAsync(mapping, cancellationToken);
    }

    public Task<UnitOfMeasureExternalMapping?> GetAsync(UnitOfMeasureMappingScheme scheme, string code, CancellationToken cancellationToken = default)
        => _dbContext.UnitOfMeasureExternalMappings.FirstOrDefaultAsync(
            mapping => mapping.Scheme == scheme
                && mapping.Code == code.Trim().ToUpperInvariant(),
            cancellationToken);
}
