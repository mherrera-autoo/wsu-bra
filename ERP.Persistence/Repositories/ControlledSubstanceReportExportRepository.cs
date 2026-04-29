using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;

namespace ERP.Persistence.Repositories;

public sealed class ControlledSubstanceReportExportRepository : IControlledSubstanceReportExportRepository
{
    private readonly ErpDbContext _dbContext;

    public ControlledSubstanceReportExportRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ControlledSubstanceReportExport export, CancellationToken cancellationToken = default)
        => await _dbContext.ControlledSubstanceReportExports.AddAsync(export, cancellationToken);
}
