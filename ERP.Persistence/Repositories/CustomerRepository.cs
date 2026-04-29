using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly ErpDbContext _dbContext;

    public CustomerRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await _dbContext.Customers.AddAsync(customer, cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(long companyId, string name, long? excludeId = null, CancellationToken cancellationToken = default)
        => _dbContext.Customers.AnyAsync(c =>
            c.CompanyId == companyId
            && c.Name == name
            && (!excludeId.HasValue || c.Id != excludeId.Value),
            cancellationToken);

    public Task<Customer?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _dbContext.Customers.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Customer>> ListAsync(long companyId, CancellationToken cancellationToken = default)
        => await _dbContext.Customers.AsNoTracking()
            .Where(c => c.CompanyId == companyId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public void Remove(Customer customer)
    {
        _dbContext.Customers.Remove(customer);
    }
}
