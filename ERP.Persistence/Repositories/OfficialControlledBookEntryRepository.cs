using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class OfficialControlledBookEntryRepository : IOfficialControlledBookEntryRepository
{
    private readonly ErpDbContext _dbContext;

    public OfficialControlledBookEntryRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> ExistsByReferenceAsync(
        long companyId,
        string referenceDocument,
        long productId,
        string batchNumber,
        ControlledMovementType movementType,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.OfficialControlledBookEntries.AnyAsync(entry =>
            entry.CompanyId == companyId &&
            entry.ReferenceDocument == referenceDocument &&
            entry.ProductId == productId &&
            entry.BatchNumber == batchNumber &&
            entry.MovementType == movementType,
            cancellationToken);
    }

    public async Task<IReadOnlyList<OfficialControlledBookEntry>> ListAsync(
        long companyId,
        DateTime? from,
        DateTime? to,
        long? productId,
        ControlledMovementType? movementType,
        string? batchNumber,
        string? ispFolio,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OfficialControlledBookEntries.AsQueryable()
            .Where(entry => entry.CompanyId == companyId);

        if (from is not null)
        {
            query = query.Where(entry => entry.Date >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(entry => entry.Date <= to.Value);
        }

        if (productId is not null)
        {
            query = query.Where(entry => entry.ProductId == productId.Value);
        }

        if (movementType is not null)
        {
            query = query.Where(entry => entry.MovementType == movementType.Value);
        }

        if (!string.IsNullOrWhiteSpace(batchNumber))
        {
            var normalizedBatch = batchNumber.Trim();
            query = query.Where(entry => entry.BatchNumber == normalizedBatch);
        }

        if (!string.IsNullOrWhiteSpace(ispFolio))
        {
            var normalizedFolio = ispFolio.Trim();
            query = query.Where(entry => entry.IspFolio == normalizedFolio);
        }

        return await query
            .OrderBy(entry => entry.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(OfficialControlledBookEntry entry, CancellationToken cancellationToken = default)
    {
        await _dbContext.OfficialControlledBookEntries.AddAsync(entry, cancellationToken);
    }
}
