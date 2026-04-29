using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class UnitOfMeasureTranslationRepository : IUnitOfMeasureTranslationRepository
{
    private readonly ErpDbContext _dbContext;

    public UnitOfMeasureTranslationRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(UnitOfMeasureTranslation translation, CancellationToken cancellationToken = default)
    {
        await _dbContext.UnitOfMeasureTranslations.AddAsync(translation, cancellationToken);
    }

    public Task<UnitOfMeasureTranslation?> GetAsync(long unitOfMeasureId, string culture, CancellationToken cancellationToken = default)
        => _dbContext.UnitOfMeasureTranslations.FirstOrDefaultAsync(
            translation => translation.UnitOfMeasureId == unitOfMeasureId
                && translation.Culture == culture,
            cancellationToken);
}
