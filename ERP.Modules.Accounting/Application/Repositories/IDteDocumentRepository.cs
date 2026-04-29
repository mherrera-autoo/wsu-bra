using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface IDteDocumentRepository
{
    Task AddAsync(DteDocument document, CancellationToken cancellationToken = default);
    Task<DteDocument?> GetByFolioAsync(long companyId, string folio, DteDocumentType documentType, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DteDocument>> ListByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DteDocument>> ListByPeriodAsync(long companyId, int year, int month, CancellationToken cancellationToken = default);
}
