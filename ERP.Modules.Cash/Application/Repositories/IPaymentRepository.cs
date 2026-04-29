using ERP.Modules.Cash.Domain;

namespace ERP.Modules.Cash.Application.Repositories;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
    Task<Payment?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
