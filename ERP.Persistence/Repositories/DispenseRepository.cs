using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class DispenseRepository : IDispenseRepository
{
    private readonly ErpDbContext _dbContext;

    public DispenseRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Dispense?> GetByIdAsync(long companyId, long dispenseId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Dispenses
            .Include(dispense => dispense.Lines)
            .Include(dispense => dispense.Prescription)
            .ThenInclude(prescription => prescription!.Items)
            .FirstOrDefaultAsync(dispense => dispense.CompanyId == companyId && dispense.Id == dispenseId, cancellationToken);
    }

    public async Task AddAsync(Dispense dispense, CancellationToken cancellationToken = default)
    {
        await _dbContext.Dispenses.AddAsync(dispense, cancellationToken);
    }
}
