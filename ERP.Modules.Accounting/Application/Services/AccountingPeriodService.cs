using ERP.Modules.Accounting.Application.Repositories;
using ERP.Shared.Application;

namespace ERP.Modules.Accounting.Application.Services;

public sealed class AccountingPeriodService
{
    private readonly IAccountingPeriodRepository _periodRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AccountingPeriodService(IAccountingPeriodRepository periodRepository, IUnitOfWork unitOfWork)
    {
        _periodRepository = periodRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<Result> ClosePeriodAsync(
        long companyId,
        int year,
        int month,
        long closedBy,
        CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var period = await _periodRepository.GetByMonthAsync(companyId, year, month, token);
            if (period is null)
            {
                return Result.Fail("Accounting period not found.");
            }

            if (period.Status == Domain.AccountingPeriodStatus.Closed)
            {
                return Result.Fail("Accounting period already closed.");
            }

            period.Close(DateTime.UtcNow, closedBy);
            return Result.Ok();
        }, cancellationToken);
    }
}
