namespace ERP.Modules.Wsu.Application.Repositories;

public interface IWsuRfidTagLookupRepository
{
    Task<IReadOnlySet<string>> ListExistingEpcsAsync(Guid companyPublicId, Guid warehousePublicId, IReadOnlyCollection<string> epcs, CancellationToken cancellationToken = default);
    Task AddMissingAsync(Guid companyPublicId, Guid warehousePublicId, IReadOnlyCollection<string> epcs, CancellationToken cancellationToken = default);
    Task DeleteByCompanyWarehouseAndEpcsAsync(Guid companyPublicId, Guid warehousePublicId, IReadOnlyCollection<string> epcs, CancellationToken cancellationToken = default);
}
