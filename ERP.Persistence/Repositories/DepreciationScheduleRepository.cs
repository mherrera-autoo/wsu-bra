using ERP.Modules.FixedAssets.Application.Repositories;
using ERP.Modules.FixedAssets.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class DepreciationScheduleRepository : IDepreciationScheduleRepository
{
    private readonly ErpDbContext _dbContext;

    public DepreciationScheduleRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(DepreciationSchedule schedule, CancellationToken cancellationToken = default)
        => await _dbContext.DepreciationSchedules.AddAsync(schedule, cancellationToken);

    public Task<DepreciationSchedule?> GetByAssetIdAsync(long companyId, long assetId, CancellationToken cancellationToken = default)
        => _dbContext.DepreciationSchedules
            .Include(schedule => schedule.Lines)
            .FirstOrDefaultAsync(schedule => schedule.CompanyId == companyId && schedule.FixedAssetId == assetId, cancellationToken);

    public Task UpdateAsync(DepreciationSchedule schedule, CancellationToken cancellationToken = default)
    {
        _dbContext.DepreciationSchedules.Update(schedule);
        return Task.CompletedTask;
    }
}
