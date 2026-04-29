using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface IUnitOfMeasureTranslationRepository
{
    Task AddAsync(UnitOfMeasureTranslation translation, CancellationToken cancellationToken = default);
    Task<UnitOfMeasureTranslation?> GetAsync(long unitOfMeasureId, string culture, CancellationToken cancellationToken = default);
}
