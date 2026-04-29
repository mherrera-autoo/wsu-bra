using ERP.Modules.Sales.Contracts;
using ERP.Modules.Sales.Domain;
using Microsoft.EntityFrameworkCore;
using ContractSalesDocumentKind = ERP.Modules.Sales.Contracts.SalesDocumentKind;
using ContractSalesDocumentStatus = ERP.Modules.Sales.Contracts.SalesDocumentStatus;

namespace ERP.Persistence.Repositories;

public sealed class SalesDocumentQueryRepository : ISalesDocumentQuery
{
    private readonly ErpDbContext _dbContext;

    public SalesDocumentQueryRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SalesDocumentSnapshot?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var document = await _dbContext.SalesDocuments
            .AsNoTracking()
            .Include(doc => doc.Lines)
            .FirstOrDefaultAsync(doc => doc.Id == id, cancellationToken);

        return document is null ? null : Map(document);
    }

    public async Task<SalesDocumentSnapshot?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
    {
        var document = await _dbContext.SalesDocuments
            .AsNoTracking()
            .Include(doc => doc.Lines)
            .FirstOrDefaultAsync(doc => doc.CompanyId == companyId && doc.Id == id, cancellationToken);

        return document is null ? null : Map(document);
    }

    public async Task<IReadOnlyList<SalesDocumentSnapshot>> ListAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var documents = await _dbContext.SalesDocuments
            .AsNoTracking()
            .Include(doc => doc.Lines)
            .Where(doc => doc.CompanyId == companyId)
            .OrderByDescending(doc => doc.Id)
            .ToListAsync(cancellationToken);

        return documents.Select(Map).ToList();
    }

    private static SalesDocumentSnapshot Map(SalesDocument document)
        => new(
            document.Id,
            document.CompanyId,
            document.CustomerId,
            (ContractSalesDocumentKind)document.Kind,
            (ContractSalesDocumentStatus)document.Status,
            document.Lines
                .Select(line => new SalesDocumentLineSnapshot(
                    line.ProductId,
                    line.Qty,
                    line.UnitPrice))
                .ToList());
}
