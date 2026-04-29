using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Services;

public sealed class UnitOfMeasureAccessValidator : IUnitOfMeasureAccessValidator
{
    private readonly ICompanyUnitOfMeasureRepository _companyUnitRepository;
    private readonly IUnitOfMeasureRepository _unitOfMeasureRepository;

    public UnitOfMeasureAccessValidator(
        ICompanyUnitOfMeasureRepository companyUnitRepository,
        IUnitOfMeasureRepository unitOfMeasureRepository)
    {
        _companyUnitRepository = companyUnitRepository;
        _unitOfMeasureRepository = unitOfMeasureRepository;
    }

    public async Task<bool> IsEnabledForCompanyAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default)
    {
        var unit = await _unitOfMeasureRepository.GetByIdAsync(unitOfMeasureId, cancellationToken);
        if (unit is null || !unit.IsActive)
        {
            return false;
        }

        return await _companyUnitRepository.IsEnabledAsync(companyId, unitOfMeasureId, cancellationToken);
    }

    public async Task EnsureEnabledForCompanyAsync(long companyId, long unitOfMeasureId, CancellationToken cancellationToken = default)
    {
        if (!await IsEnabledForCompanyAsync(companyId, unitOfMeasureId, cancellationToken))
        {
            throw new UnitOfMeasureAccessException("Unit of measure is not enabled for the company.");
        }
    }
}
