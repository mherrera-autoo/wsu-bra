using ERP.Modules.Purchasing.Domain;

namespace ERP.Modules.Purchasing.Application.Repositories;

public interface IGoodsReceiptRepository
{
    Task AddAsync(GoodsReceipt goodsReceipt, CancellationToken cancellationToken = default);
    Task<GoodsReceipt?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GoodsReceipt>> ListAsync(long companyId, CancellationToken cancellationToken = default);
}
