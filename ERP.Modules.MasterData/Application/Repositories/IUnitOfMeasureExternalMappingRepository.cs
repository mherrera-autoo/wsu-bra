using ERP.Modules.MasterData.Domain;

namespace ERP.Modules.MasterData.Application.Repositories;

public interface IUnitOfMeasureExternalMappingRepository
{
    Task AddAsync(UnitOfMeasureExternalMapping mapping, CancellationToken cancellationToken = default);
    Task<UnitOfMeasureExternalMapping?> GetAsync(UnitOfMeasureMappingScheme scheme, string code, CancellationToken cancellationToken = default);
}
