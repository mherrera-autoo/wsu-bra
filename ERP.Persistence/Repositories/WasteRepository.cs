using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;

namespace ERP.Persistence.Repositories;

public sealed class WasteRepository : IWasteRepository
{
    private readonly ErpDbContext _dbContext;

    public WasteRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Waste waste, CancellationToken cancellationToken = default)
    {
        await _dbContext.Wastes.AddAsync(waste, cancellationToken);
    }
}
