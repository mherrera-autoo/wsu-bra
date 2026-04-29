using ERP.Modules.Inventory.Domain;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Reporting;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Reports;

public sealed class PharmacyReportsQuery : IPharmacyReportsQuery
{
    private readonly ErpDbContext _dbContext;

    public PharmacyReportsQuery(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ExpiringStockReportLine>> GetExpiringStockAsync(
        long companyId,
        ExpiringStockReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.Date;

        // OJO: no usar "from" como variable porque choca con LINQ query syntax (from ...)
        var fromDate = query.From ?? now;
        var toDate = query.To ?? now.AddDays(query.DaysThreshold ?? 30);

        var expiringQuery =
            from batch in _dbContext.StockBatches.AsNoTracking()
            join product in _dbContext.Products.AsNoTracking() on batch.ProductId equals product.Id
            join warehouse in _dbContext.Warehouses.AsNoTracking() on batch.WarehouseId equals warehouse.Id
            where batch.CompanyId == companyId
                && product.CompanyId == companyId
                && warehouse.CompanyId == companyId
                && batch.ExpiryDate != null
                && batch.QuantityOnHand > 0
                && batch.ExpiryDate >= fromDate
                && batch.ExpiryDate <= toDate
            select new
            {
                batch.ProductId,
                product.Sku,
                product.Name,
                batch.WarehouseId,
                WarehouseName = warehouse.Name,
                batch.BatchNumber,
                batch.ExpiryDate,
                batch.QuantityOnHand,
                batch.HealthStatus
            };

        if (query.ProductId.HasValue)
        {
            expiringQuery = expiringQuery.Where(item => item.ProductId == query.ProductId.Value);
        }

        if (query.WarehouseId.HasValue)
        {
            expiringQuery = expiringQuery.Where(item => item.WarehouseId == query.WarehouseId.Value);
        }

        if (query.HealthStatus.HasValue)
        {
            expiringQuery = expiringQuery.Where(item => item.HealthStatus == query.HealthStatus.Value);
        }

        var rows = await expiringQuery.ToListAsync(cancellationToken);

        return rows
            .Select(row => new ExpiringStockReportLine(
                row.ProductId,
                row.Sku,
                row.Name,
                row.WarehouseId,
                row.WarehouseName,
                row.BatchNumber,
                row.ExpiryDate!.Value,
                row.QuantityOnHand,
                row.HealthStatus,
                (int)Math.Ceiling((row.ExpiryDate!.Value.Date - now).TotalDays)))
            .OrderBy(line => line.ExpiryDate)
            .ThenBy(line => line.ProductName)
            .ToList();
    }

    public async Task<IReadOnlyList<ControlledSubstanceReportLine>> GetControlledSubstancesAsync(
        long companyId,
        ControlledSubstanceReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var controlledQuery =
            from entry in _dbContext.OfficialControlledBookEntries.AsNoTracking()
            join product in _dbContext.Products.AsNoTracking() on entry.ProductId equals product.Id
            where entry.CompanyId == companyId
                && product.CompanyId == companyId
            select new
            {
                entry.Date,
                entry.ProductId,
                product.Sku,
                product.Name,
                entry.BatchNumber,
                entry.Quantity,
                entry.MovementType,
                entry.ReferenceDocument,
                entry.CreatedBy,
                entry.IspFolio,
                entry.IspReason,
                entry.IspOriginDestination,
                entry.IspSupplierOrPatient,
                entry.IspPrescriber,
                entry.IspTaxReference
            };

        if (query.From.HasValue)
        {
            controlledQuery = controlledQuery.Where(entry => entry.Date >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            controlledQuery = controlledQuery.Where(entry => entry.Date <= query.To.Value);
        }

        if (query.ProductId.HasValue)
        {
            controlledQuery = controlledQuery.Where(entry => entry.ProductId == query.ProductId.Value);
        }

        if (query.MovementType.HasValue)
        {
            controlledQuery = controlledQuery.Where(entry => entry.MovementType == query.MovementType.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.BatchNumber))
        {
            var normalizedBatch = query.BatchNumber.Trim();
            controlledQuery = controlledQuery.Where(entry => entry.BatchNumber == normalizedBatch);
        }

        var rows = await controlledQuery
            .OrderBy(entry => entry.Date)
            .ThenBy(entry => entry.BatchNumber)
            .ToListAsync(cancellationToken);

        return rows
            .Select(entry => new ControlledSubstanceReportLine(
                entry.Date,
                entry.ProductId,
                entry.Sku,
                entry.Name,
                entry.BatchNumber,
                entry.Quantity,
                entry.MovementType,
                entry.ReferenceDocument,
                entry.CreatedBy,
                entry.IspFolio,
                entry.IspReason,
                entry.IspOriginDestination,
                entry.IspSupplierOrPatient,
                entry.IspPrescriber,
                entry.IspTaxReference))
            .ToList();
    }

    public async Task<IReadOnlyList<WasteReportLine>> GetWasteAsync(
        long companyId,
        WasteReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var wasteQuery =
            from waste in _dbContext.Wastes.AsNoTracking()
            join batch in _dbContext.StockBatches.AsNoTracking() on waste.StockBatchId equals batch.Id
            join product in _dbContext.Products.AsNoTracking() on batch.ProductId equals product.Id
            join warehouse in _dbContext.Warehouses.AsNoTracking() on batch.WarehouseId equals warehouse.Id
            where waste.CompanyId == companyId
                && batch.CompanyId == companyId
                && product.CompanyId == companyId
                && warehouse.CompanyId == companyId
            select new
            {
                waste.CreatedAt,
                batch.ProductId,
                product.Sku,
                product.Name,
                batch.WarehouseId,
                WarehouseName = warehouse.Name,
                batch.BatchNumber,
                waste.Quantity,
                waste.Reason,
                waste.ReportedByUserId
            };

        if (query.From.HasValue)
        {
            wasteQuery = wasteQuery.Where(w => w.CreatedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            wasteQuery = wasteQuery.Where(w => w.CreatedAt <= query.To.Value);
        }

        if (query.ProductId.HasValue)
        {
            wasteQuery = wasteQuery.Where(w => w.ProductId == query.ProductId.Value);
        }

        if (query.WarehouseId.HasValue)
        {
            wasteQuery = wasteQuery.Where(w => w.WarehouseId == query.WarehouseId.Value);
        }

        var rows = await wasteQuery
            .OrderByDescending(w => w.CreatedAt)
            .ThenBy(w => w.ProductId)
            .ToListAsync(cancellationToken);

        return rows
            .Select(w => new WasteReportLine(
                w.CreatedAt,
                w.ProductId,
                w.Sku,
                w.Name,
                w.WarehouseId,
                w.WarehouseName,
                w.BatchNumber,
                w.Quantity,
                w.Reason,
                w.ReportedByUserId))
            .ToList();
    }

    public async Task<IReadOnlyList<InventoryDifferenceReportLine>> GetInventoryDifferencesAsync(
        long companyId,
        InventoryDifferenceReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var differencesQuery =
            from movement in _dbContext.InventoryMovements.AsNoTracking()
            join product in _dbContext.Products.AsNoTracking() on movement.ProductId equals product.Id
            join warehouse in _dbContext.Warehouses.AsNoTracking()
                on (movement.ToWarehouseId ?? movement.FromWarehouseId) equals warehouse.Id
            where movement.CompanyId == companyId
                && product.CompanyId == companyId
                && warehouse.CompanyId == companyId
                && movement.MovementType == MovementType.Adjustment
            select new
            {
                movement.CreatedAt,
                movement.ProductId,
                product.Sku,
                product.Name,
                WarehouseId = movement.ToWarehouseId ?? movement.FromWarehouseId,
                WarehouseName = warehouse.Name,
                movement.Quantity,
                IsIncrease = movement.ToWarehouseId != null,
                movement.ReferenceType,
                movement.ReferenceId,
                movement.Reason
            };

        if (query.From.HasValue)
        {
            differencesQuery = differencesQuery.Where(m => m.CreatedAt >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            differencesQuery = differencesQuery.Where(m => m.CreatedAt <= query.To.Value);
        }

        if (query.ProductId.HasValue)
        {
            differencesQuery = differencesQuery.Where(m => m.ProductId == query.ProductId.Value);
        }

        if (query.WarehouseId.HasValue)
        {
            differencesQuery = differencesQuery.Where(m => m.WarehouseId == query.WarehouseId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ReferenceType))
        {
            var normalizedReference = query.ReferenceType.Trim();
            differencesQuery = differencesQuery.Where(m => m.ReferenceType == normalizedReference);
        }

        var rows = await differencesQuery
            .OrderByDescending(m => m.CreatedAt)
            .ThenBy(m => m.ProductId)
            .ToListAsync(cancellationToken);

        return rows
            .Select(m => new InventoryDifferenceReportLine(
                m.CreatedAt,
                m.ProductId,
                m.Sku,
                m.Name,
                m.WarehouseId ?? 0L,
                m.WarehouseName,
                m.Quantity,
                m.IsIncrease,
                m.ReferenceType,
                m.ReferenceId,
                m.Reason))
            .ToList();
    }

    public async Task<IReadOnlyList<StockoutReportLine>> GetStockoutsAsync(
        long companyId,
        StockoutReportQuery query,
        CancellationToken cancellationToken = default)
    {
        var thresholds = await _dbContext.StockoutThresholds
            .AsNoTracking()
            .Where(t => t.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        var stockQuery =
            from stock in _dbContext.Stocks.AsNoTracking()
            join product in _dbContext.Products.AsNoTracking() on stock.ProductId equals product.Id
            join warehouse in _dbContext.Warehouses.AsNoTracking() on stock.WarehouseId equals warehouse.Id
            where stock.CompanyId == companyId
                && product.CompanyId == companyId
                && warehouse.CompanyId == companyId
            select new
            {
                stock.ProductId,
                product.Sku,
                product.Name,
                stock.WarehouseId,
                WarehouseName = warehouse.Name,
                stock.OnHandQuantity
            };

        if (query.ProductId.HasValue)
        {
            stockQuery = stockQuery.Where(s => s.ProductId == query.ProductId.Value);
        }

        if (query.WarehouseId.HasValue)
        {
            stockQuery = stockQuery.Where(s => s.WarehouseId == query.WarehouseId.Value);
        }

        var stocks = await stockQuery.ToListAsync(cancellationToken);

        var defaultThresholds = thresholds
            .Where(t => t.WarehouseId == null)
            .ToDictionary(t => t.ProductId, t => t.ThresholdQuantity);

        var specificThresholds = thresholds
            .Where(t => t.WarehouseId != null)
            .ToDictionary(t => (t.ProductId, t.WarehouseId!.Value), t => t.ThresholdQuantity);

        var lines = new List<StockoutReportLine>();

        foreach (var stock in stocks)
        {
            if (!specificThresholds.TryGetValue((stock.ProductId, stock.WarehouseId), out var threshold))
            {
                if (!defaultThresholds.TryGetValue(stock.ProductId, out threshold))
                {
                    continue;
                }
            }

            if (stock.OnHandQuantity <= threshold)
            {
                lines.Add(new StockoutReportLine(
                    stock.ProductId,
                    stock.Sku,
                    stock.Name,
                    stock.WarehouseId,
                    stock.WarehouseName,
                    stock.OnHandQuantity,
                    threshold));
            }
        }

        return lines
            .OrderBy(line => line.ProductName)
            .ThenBy(line => line.WarehouseName)
            .ToList();
    }
}
