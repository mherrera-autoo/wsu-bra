using ERP.Modules.Cash.Application.Repositories;
using ERP.Modules.Cash.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PaymentRepository : IPaymentRepository
{
    private readonly ErpDbContext _dbContext;

    public PaymentRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        await _dbContext.Payments.AddAsync(payment, cancellationToken);
    }

    public Task<Payment?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}
