using ERP.Modules.Purchasing.Domain;

namespace ERP.Modules.Purchasing.Application.Repositories;

public interface IPurchaseSuggestionRepository
{
    Task AddAsync(PurchaseSuggestion suggestion, CancellationToken cancellationToken = default);
    Task<PurchaseSuggestion?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PurchaseSuggestion>> ListAsync(
        long companyId,
        long? warehouseId,
        long? supplierId,
        PurchaseSuggestionStatus? status,
        CancellationToken cancellationToken = default);
}
