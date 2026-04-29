using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> ListAsync(long companyId, CancellationToken cancellationToken = default);
    void Remove(Customer customer);
}
