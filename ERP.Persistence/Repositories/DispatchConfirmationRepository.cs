using ERP.Modules.Wms.Application.Repositories;
using ERP.Modules.Wms.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class DispatchConfirmationRepository : IDispatchConfirmationRepository
{
    private readonly ErpDbContext _dbContext;

    public DispatchConfirmationRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(DispatchConfirmation confirmation, CancellationToken cancellationToken = default)
    {
        await _dbContext.DispatchConfirmations.AddAsync(confirmation, cancellationToken);
    }

    public Task<DispatchConfirmation?> GetByReferenceAsync(
        long companyId,
        string referenceType,
        string referenceId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.DispatchConfirmations.FirstOrDefaultAsync(
            confirmation => confirmation.CompanyId == companyId
                && confirmation.ReferenceType == referenceType
                && confirmation.ReferenceId == referenceId,
            cancellationToken);
    }
}
