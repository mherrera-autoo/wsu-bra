using ERP.Modules.Purchasing.Application.Repositories;
using ERP.Modules.Purchasing.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PurchaseSuggestionRepository : IPurchaseSuggestionRepository
{
    private readonly ErpDbContext _dbContext;

    public PurchaseSuggestionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PurchaseSuggestion suggestion, CancellationToken cancellationToken = default)
    {
        await _dbContext.PurchaseSuggestions.AddAsync(suggestion, cancellationToken);
    }

    public Task<PurchaseSuggestion?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.PurchaseSuggestions
            .Include(suggestion => suggestion.Lines)
            .FirstOrDefaultAsync(suggestion => suggestion.CompanyId == companyId && suggestion.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PurchaseSuggestion>> ListAsync(
        long companyId,
        long? warehouseId,
        long? supplierId,
        PurchaseSuggestionStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PurchaseSuggestions
            .AsNoTracking()
            .Include(suggestion => suggestion.Lines)
            .Where(suggestion => suggestion.CompanyId == companyId);

        if (status.HasValue)
        {
            query = query.Where(suggestion => suggestion.Status == status.Value);
        }

        if (supplierId.HasValue)
        {
            query = query.Where(suggestion => suggestion.SupplierSuggestedId == supplierId.Value
                || suggestion.Lines.Any(line => line.SupplierSuggestedId == supplierId.Value));
        }

        if (warehouseId.HasValue)
        {
            query = query.Where(suggestion => suggestion.Lines.Any(line => line.WarehouseId == warehouseId.Value));
        }

        return await query
            .OrderByDescending(suggestion => suggestion.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
