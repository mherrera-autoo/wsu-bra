using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class DeliveryConfirmationRepository : IDeliveryConfirmationRepository
{
    private readonly ErpDbContext _dbContext;

    public DeliveryConfirmationRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(DeliveryConfirmation confirmation, CancellationToken cancellationToken = default)
        => await _dbContext.DeliveryConfirmations.AddAsync(confirmation, cancellationToken);

    public async Task<IReadOnlyList<DeliveryConfirmation>> ListByReferenceAsync(
        long companyId,
        string referenceType,
        string referenceId,
        CancellationToken cancellationToken = default)
    {
        var normalizedType = referenceType.Trim();
        var normalizedId = referenceId.Trim();

        return await _dbContext.DeliveryConfirmations
            .AsNoTracking()
            .Where(c =>
                c.CompanyId == companyId &&
                c.ReferenceType == normalizedType &&
                c.ReferenceId == normalizedId)
            .OrderByDescending(c => c.DeliveredAt)
            .ToListAsync(cancellationToken);
    }
}
