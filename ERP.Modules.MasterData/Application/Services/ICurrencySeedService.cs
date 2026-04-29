using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public interface ICurrencySeedService
{
    Task<Result> SeedAsync(CancellationToken cancellationToken = default);
}