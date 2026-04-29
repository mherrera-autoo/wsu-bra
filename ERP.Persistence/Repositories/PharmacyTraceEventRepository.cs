using System.Linq;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Repositories;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class PharmacyTraceEventRepository : IPharmacyTraceEventRepository
{
    private readonly ErpDbContext _dbContext;

    public PharmacyTraceEventRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(PharmacyTraceEvent traceEvent, CancellationToken cancellationToken = default)
    {
        await _dbContext.PharmacyTraceEvents.AddAsync(traceEvent, cancellationToken);
    }

    public async Task<IReadOnlyList<PharmacyTraceEvent>> ListByBatchAsync(long companyId, string batchNumber, CancellationToken cancellationToken = default)
    {
        var normalized = batchNumber.Trim();
        return await _dbContext.PharmacyTraceEvents
            .AsNoTracking()
            .Where(trace => trace.CompanyId == companyId && trace.BatchNumber == normalized)
            .OrderByDescending(trace => trace.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PharmacyTraceEvent>> ListByPatientAsync(long companyId, string patientReference, CancellationToken cancellationToken = default)
    {
        var normalized = patientReference.Trim();
        return await _dbContext.PharmacyTraceEvents
            .AsNoTracking()
            .Where(trace => trace.CompanyId == companyId && trace.PatientReference == normalized)
            .OrderByDescending(trace => trace.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PharmacyTraceEvent>> ListBySupplierAsync(long companyId, long supplierId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.PharmacyTraceEvents
            .AsNoTracking()
            .Where(trace => trace.CompanyId == companyId && trace.SupplierId == supplierId)
            .OrderByDescending(trace => trace.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
