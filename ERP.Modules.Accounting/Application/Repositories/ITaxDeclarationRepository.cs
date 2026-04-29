using ERP.Modules.Accounting.Domain;

namespace ERP.Modules.Accounting.Application.Repositories;

public interface ITaxDeclarationRepository
{
    Task AddAsync(TaxDeclaration declaration, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TaxDeclaration>> ListByCompanyAsync(long companyId, CancellationToken cancellationToken = default);
}
