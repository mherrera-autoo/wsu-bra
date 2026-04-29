using ERP.Modules.MasterData.Contracts;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ITaxEntityRepository
{
    Task<TaxEntitySnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<TaxEntitySnapshot?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<TaxEntitySnapshot?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);
    Task<TaxEntitySnapshot> AddAsync(TaxEntityCreateRequest taxEntity, CancellationToken cancellationToken = default);
}
