namespace ERP.Modules.MasterData.Application.Services;

public interface IUnitOfMeasureAccessValidator
{
    Task<bool> IsEnabledForCompanyAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default);
    Task EnsureEnabledForCompanyAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default);
}
