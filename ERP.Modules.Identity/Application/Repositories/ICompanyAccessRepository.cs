using ERP.Modules.Identity.Contracts;

namespace ERP.Modules.Identity.Application.Repositories;

public interface ICompanyAccessRepository
{
    Task<IReadOnlyList<CompanyAccessSummary>> ListByUserAsync(long userId, CancellationToken cancellationToken = default);
}
