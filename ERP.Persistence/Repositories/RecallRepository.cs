using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;

namespace ERP.Persistence.Repositories;

public sealed class RecallRepository : IRecallRepository
{
    private readonly ErpDbContext _dbContext;

    public RecallRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Recall recall, CancellationToken cancellationToken = default)
    {
        await _dbContext.Recalls.AddAsync(recall, cancellationToken);
    }
}
