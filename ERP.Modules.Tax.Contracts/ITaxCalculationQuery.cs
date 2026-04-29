using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Modules.Tax.Contracts;

public interface ITaxCalculationQuery
{
    Task<Result<Money>> CalculateTaxAmountAsync(
        long companyId,
        long? taxGroupId,
        decimal baseAmount,
        string currency,
        CancellationToken cancellationToken = default);
}
