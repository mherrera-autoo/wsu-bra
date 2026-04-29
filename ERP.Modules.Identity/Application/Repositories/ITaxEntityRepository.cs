using ERP.Modules.Identity.Contracts;
using ERP.Modules.MasterData.Contracts;

namespace ERP.Modules.Identity.Application.Repositories;

public interface ITaxEntityRepository
{
    Task<TaxEntitySnapshot?> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default);
    Task<TaxEntitySnapshot?> GetByTaxIdAsync(string taxId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxEntitySnapshot>> ListByCompanyPublicIdAsync(Guid companyPublicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyTaxEntityInfo>> ListByCompanyIdsAsync(IReadOnlyCollection<long> companyIds, CancellationToken cancellationToken = default);
    Task<TaxEntitySnapshot> AddAsync(TaxEntityCreateRequest taxEntity, CancellationToken cancellationToken = default);
}
