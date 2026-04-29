using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Modules.Accounting.Application.Services;
using ERP.Modules.Pricing.Application.Services;
using ERP.Modules.Purchasing.Application.Services;
using ERP.Modules.Sales.Application.Services;
using ERP.Persistence;
using ERP.Shared.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ERP.Api.Services;

public sealed class DevSeedService
{
    private static readonly IReadOnlyList<DevSeedCompany> DemoCompanies =
        [
            new DevSeedCompany(1001, "Comercial Los Andes SpA", "comercial-los-andes", Guid.Parse("11111111-1111-1111-1111-111111111111")),
            new DevSeedCompany(1002, "Servicios Punta Loma Ltda.", "servicios-punta-loma", Guid.Parse("22222222-2222-2222-2222-222222222222")),
            new DevSeedCompany(1003, "Importadora Río Claro SpA", "importadora-rio-claro", Guid.Parse("33333333-3333-3333-3333-333333333333"))
        ];

    private readonly ErpDbContext _dbContext;
    private readonly MasterDataService _masterDataService;
    private readonly IUnitOfMeasureSeedService _unitOfMeasureSeedService;
    private readonly ICurrencySeedService _currencySeedService;
    private readonly PurchasingService _purchasingService;
    private readonly SalesService _salesService;
    private readonly PricingService _pricingService;
    private readonly AccountingBootstrapService _accountingBootstrapService;

    public DevSeedService(
        ErpDbContext dbContext,
        MasterDataService masterDataService,
        IUnitOfMeasureSeedService unitOfMeasureSeedService,
        ICurrencySeedService currencySeedService,
        PurchasingService purchasingService,
        SalesService salesService,
        PricingService pricingService,
        AccountingBootstrapService accountingBootstrapService)
    {
        _dbContext = dbContext;
        _masterDataService = masterDataService;
        _unitOfMeasureSeedService = unitOfMeasureSeedService;
        _currencySeedService = currencySeedService;
        _purchasingService = purchasingService;
        _salesService = salesService;
        _pricingService = pricingService;
        _accountingBootstrapService = accountingBootstrapService;
    }

    public async Task<DevSeedResult> SeedBaseAsync(bool force, CancellationToken cancellationToken = default)
    {
        EnsureSuccess(await _unitOfMeasureSeedService.SeedGlobalStandardUnitsAsync(cancellationToken));
        EnsureSuccess(await _currencySeedService.SeedAsync(cancellationToken));

        var results = new List<DevSeedCompanyResult>();
        var seededAny = false;
        foreach (var company in DemoCompanies)
        {
            await EnsureCompanyExistsAsync(company, cancellationToken);
            await EnsureCompanyCurrencyAsync(company.CompanyId, "CLP", cancellationToken);

            var hasBaseData = await HasBaseDataAsync(company.CompanyId, cancellationToken);
            if (hasBaseData && !force)
            {
                results.Add(new DevSeedCompanyResult(company.CompanyId, company.Name, company.Slug, "skipped"));
                continue;
            }

            if (hasBaseData)
            {
                await DeleteCompanyDataAsync(company.CompanyId, cancellationToken);
                await EnsureCompanyCurrencyAsync(company.CompanyId, "CLP", cancellationToken);
            }

            await SeedCompanyBaseAsync(company.CompanyId, cancellationToken);
            results.Add(new DevSeedCompanyResult(company.CompanyId, company.Name, company.Slug, "seeded"));
            seededAny = true;
        }

        return new DevSeedResult(
            seededAny,
            force,
            results,
            seededAny ? "Base demo data seeded." : "Base demo data already exists. Use ?force=true to reseed.");
    }

    public async Task<DevSeedResult> SeedAccountingAsync(bool force, CancellationToken cancellationToken = default)
    {
        await EnsureDemoCompaniesExistAsync(cancellationToken);

        var results = new List<DevSeedCompanyResult>();
        var seededAny = false;
        foreach (var company in DemoCompanies)
        {
            await EnsureCompanyCurrencyAsync(company.CompanyId, "CLP", cancellationToken);

            var hasAccountingData = await HasAccountingDataAsync(company.CompanyId, cancellationToken);
            if (hasAccountingData && !force)
            {
                results.Add(new DevSeedCompanyResult(company.CompanyId, company.Name, company.Slug, "skipped"));
                continue;
            }

            EnsureSuccess(await _accountingBootstrapService.EnsureCompanySeededAsync(
                company.CompanyId,
                "CLP",
                cancellationToken));

            results.Add(new DevSeedCompanyResult(company.CompanyId, company.Name, company.Slug, "seeded"));
            seededAny = true;
        }

        return new DevSeedResult(
            seededAny,
            force,
            results,
            seededAny ? "Accounting demo data seeded." : "Accounting demo data already exists. Use ?force=true to reseed.");
    }

    public async Task<DevSeedResult> SeedPricesAsync(bool force, CancellationToken cancellationToken = default)
    {
        await EnsureDemoCompaniesExistAsync(cancellationToken);

        var results = new List<DevSeedCompanyResult>();
        var seededAny = false;
        foreach (var company in DemoCompanies)
        {
            await EnsureBaseDataExistsAsync(company, cancellationToken);

            var hasCompletePrices = await HasCompletePricesAsync(company.CompanyId, cancellationToken);
            if (hasCompletePrices && !force)
            {
                results.Add(new DevSeedCompanyResult(company.CompanyId, company.Name, company.Slug, "skipped"));
                continue;
            }

            if (force)
            {
                await DeletePriceDataAsync(company.CompanyId, cancellationToken);
            }

            await SeedCompanyPricesAsync(company.CompanyId, cancellationToken);
            results.Add(new DevSeedCompanyResult(company.CompanyId, company.Name, company.Slug, "seeded"));
            seededAny = true;
        }

        return new DevSeedResult(
            seededAny,
            force,
            results,
            seededAny ? "Price demo data seeded." : "Price demo data already exists. Use ?force=true to reseed.");
    }

    public async Task<DevSeedResult> SeedOperationAsync(bool force, CancellationToken cancellationToken = default)
    {
        await EnsureDemoCompaniesExistAsync(cancellationToken);

        var results = new List<DevSeedCompanyResult>();
        var seededAny = false;
        foreach (var company in DemoCompanies)
        {
            await EnsureBaseDataExistsAsync(company, cancellationToken);
            await EnsureAccountingDataExistsAsync(company, cancellationToken);
            await EnsurePriceDataExistsAsync(company, cancellationToken);

            var hasOperationData = await HasOperationDataAsync(company.CompanyId, cancellationToken);
            if (hasOperationData && !force)
            {
                results.Add(new DevSeedCompanyResult(company.CompanyId, company.Name, company.Slug, "skipped"));
                continue;
            }

            if (force)
            {
                await DeleteOperationDataAsync(company.CompanyId, cancellationToken);
            }

            await SeedCompanyOperationAsync(company.CompanyId, cancellationToken);
            results.Add(new DevSeedCompanyResult(company.CompanyId, company.Name, company.Slug, "seeded"));
            seededAny = true;
        }

        return new DevSeedResult(
            seededAny,
            force,
            results,
            seededAny ? "Operation demo data seeded." : "Operation demo data already exists. Use ?force=true to reseed.");
    }

    private async Task EnsureDemoCompaniesExistAsync(CancellationToken cancellationToken)
    {
        foreach (var company in DemoCompanies)
        {
            var exists = await _dbContext.Companies
                .AsNoTracking()
                .AnyAsync(existing => existing.Id == company.CompanyId, cancellationToken);
            if (!exists)
            {
                throw new InvalidOperationException("Base demo data is missing. Run /api/dev/seed/base first.");
            }
        }
    }

    private async Task EnsureCompanyExistsAsync(DevSeedCompany company, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Companies
            .AsNoTracking()
            .AnyAsync(existing => existing.Id == company.CompanyId, cancellationToken);

        if (exists)
        {
            return;
        }

        var organization = Organization.Create(OrganizationType.Individual, company.Name);
        _dbContext.Entry(organization).Property(e => e.Id).CurrentValue = company.CompanyId;
        await _dbContext.Organizations.AddAsync(organization, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create company first to get PublicId
        var entity = Company.Create(organization.Id, 0, company.Name);
        _dbContext.Entry(entity).Property(e => e.Id).CurrentValue = company.CompanyId;
        _dbContext.Entry(entity).Property(e => e.PublicId).CurrentValue = company.CompanyPublicId;
        await _dbContext.Companies.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var taxId = $"{company.CompanyId:D8}-0";
        var taxEntity = TaxEntity.Create(entity.PublicId, taxId, company.Name);
        await _dbContext.TaxEntities.AddAsync(taxEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Update company with correct TaxEntityId
        entity.UpdateTaxEntityId(taxEntity.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCompanyCurrencyAsync(long companyId, string currencyCode, CancellationToken cancellationToken)
    {
        var trimmedCode = currencyCode.Trim();
        var currency = await _dbContext.Currencies
            .FirstOrDefaultAsync(item => item.Code == trimmedCode, cancellationToken);

        if (currency is null)
        {
            throw new InvalidOperationException($"Currency code '{trimmedCode}' was not found.");
        }

        var companyCurrency = await _dbContext.CompanyCurrencies
            .FirstOrDefaultAsync(item => item.CompanyId == companyId && item.CurrencyId == currency.Id, cancellationToken);

        if (companyCurrency is null)
        {
            var hasDefault = await _dbContext.CompanyCurrencies
                .AnyAsync(item => item.CompanyId == companyId && item.IsDefault, cancellationToken);
            companyCurrency = CompanyCurrency.Create(companyId, currency.Id, isDefault: !hasDefault, isActive: true);
            await _dbContext.CompanyCurrencies.AddAsync(companyCurrency, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var shouldSave = false;
        if (!companyCurrency.IsActive)
        {
            companyCurrency.Activate();
            shouldSave = true;
        }

        if (!companyCurrency.IsDefault)
        {
            var hasDefault = await _dbContext.CompanyCurrencies
                .AnyAsync(item => item.CompanyId == companyId && item.IsDefault, cancellationToken);
            if (!hasDefault)
            {
                companyCurrency.SetAsDefault();
                shouldSave = true;
            }
        }

        if (shouldSave)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<bool> HasBaseDataAsync(long companyId, CancellationToken cancellationToken)
    {
        var hasProducts = await _dbContext.Products
            .AsNoTracking()
            .AnyAsync(product => product.CompanyId == companyId, cancellationToken);
        if (hasProducts)
        {
            return true;
        }

        var hasWarehouses = await _dbContext.Warehouses
            .AsNoTracking()
            .AnyAsync(warehouse => warehouse.CompanyId == companyId, cancellationToken);
        if (hasWarehouses)
        {
            return true;
        }

        var hasCustomers = await _dbContext.Customers
            .AsNoTracking()
            .AnyAsync(customer => customer.CompanyId == companyId, cancellationToken);
        if (hasCustomers)
        {
            return true;
        }

        return await _dbContext.Suppliers
            .AsNoTracking()
            .AnyAsync(supplier => supplier.CompanyId == companyId, cancellationToken);
    }

    private async Task<bool> HasAccountingDataAsync(long companyId, CancellationToken cancellationToken)
    {
        var hasSettings = await _dbContext.CompanyAccountingSettings
            .AsNoTracking()
            .AnyAsync(settings => settings.CompanyId == companyId, cancellationToken);
        if (hasSettings)
        {
            return true;
        }

        return await _dbContext.Journals
            .AsNoTracking()
            .AnyAsync(journal => journal.CompanyId == companyId, cancellationToken);
    }

    private async Task<bool> HasCompletePricesAsync(long companyId, CancellationToken cancellationToken)
    {
        var requiredSkus = BuildProductSeeds().Take(10).Select(seed => seed.Sku).ToArray();
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.CompanyId == companyId && requiredSkus.Contains(product.Sku))
            .Select(product => product.Id)
            .ToListAsync(cancellationToken);

        if (products.Count != requiredSkus.Length)
        {
            return false;
        }

        var priceList = await _dbContext.PriceLists
            .AsNoTracking()
            .FirstOrDefaultAsync(list => list.CompanyId == companyId && list.Name == "Lista General", cancellationToken);
        if (priceList is null)
        {
            return false;
        }

        var pricedProductIds = await _dbContext.PriceListItems
            .AsNoTracking()
            .Where(item => item.CompanyId == companyId && item.PriceListId == priceList.Id)
            .Select(item => item.ProductId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var pricedProductSet = pricedProductIds.ToHashSet();
        return products.All(productId => pricedProductSet.Contains(productId));
    }

    private async Task<bool> HasOperationDataAsync(long companyId, CancellationToken cancellationToken)
    {
        var hasPurchaseOrders = await _dbContext.PurchaseOrders
            .AsNoTracking()
            .AnyAsync(order => order.CompanyId == companyId, cancellationToken);
        if (hasPurchaseOrders)
        {
            return true;
        }

        var hasGoodsReceipts = await _dbContext.GoodsReceipts
            .AsNoTracking()
            .AnyAsync(receipt => receipt.CompanyId == companyId, cancellationToken);
        if (hasGoodsReceipts)
        {
            return true;
        }

        return await _dbContext.SalesDocuments
            .AsNoTracking()
            .AnyAsync(document => document.CompanyId == companyId, cancellationToken);
    }

    private async Task EnsureBaseDataExistsAsync(DevSeedCompany company, CancellationToken cancellationToken)
    {
        var hasBaseData = await HasBaseDataAsync(company.CompanyId, cancellationToken);
        if (!hasBaseData)
        {
            throw new InvalidOperationException($"Base demo data is missing for company {company.CompanyId}. Run /api/dev/seed/base first.");
        }
    }

    private async Task EnsureAccountingDataExistsAsync(DevSeedCompany company, CancellationToken cancellationToken)
    {
        var hasAccountingData = await HasAccountingDataAsync(company.CompanyId, cancellationToken);
        if (!hasAccountingData)
        {
            throw new InvalidOperationException($"Accounting demo data is missing for company {company.CompanyId}. Run /api/dev/seed/accounting first.");
        }
    }

    private async Task EnsurePriceDataExistsAsync(DevSeedCompany company, CancellationToken cancellationToken)
    {
        var hasPrices = await HasCompletePricesAsync(company.CompanyId, cancellationToken);
        if (!hasPrices)
        {
            throw new InvalidOperationException($"Price demo data is missing for company {company.CompanyId}. Run /api/dev/seed/prices first.");
        }
    }

    private async Task SeedCompanyBaseAsync(long companyId, CancellationToken cancellationToken)
    {
        var units = EnsureSuccess(await _unitOfMeasureSeedService.EnableStandardUnitsForCompanyAsync(
            companyId,
            cancellationToken));
        var unitUn = units.First(unit => unit.CanonicalCode == "ea");
        var unitKg = units.First(unit => unit.CanonicalCode == "kg");

        _ = EnsureSuccess(await _masterDataService.CreateWarehouseAsync(
            companyId,
            "CENTRAL",
            "Bodega Central",
            cancellationToken));
        _ = EnsureSuccess(await _masterDataService.CreateWarehouseAsync(
            companyId,
            "TIENDA",
            "Tienda",
            cancellationToken));

        var productSeeds = BuildProductSeeds();
        foreach (var seed in productSeeds)
        {
            var unitId = seed.UnitCode == "KG" ? unitKg.Id : unitUn.Id;
            _ = EnsureSuccess(await _masterDataService.CreateProductAsync(
                companyId,
                seed.Sku,
                seed.Name,
                seed.Barcode,
                unitId,
                seed.IsStockable,
                seed.IsSellable,
                seed.IsPurchasable,
                cancellationToken));
        }

        foreach (var customerSeed in BuildCustomerSeeds())
        {
            _ = EnsureSuccess(await _masterDataService.CreateCustomerAsync(
                companyId,
                customerSeed.Name,
                customerSeed.TaxId,
                cancellationToken));
        }

        foreach (var supplierSeed in BuildSupplierSeeds())
        {
            _ = EnsureSuccess(await _masterDataService.CreateSupplierAsync(
                companyId,
                supplierSeed.Name,
                supplierSeed.TaxId,
                "Chile",
                "CLP",
                cancellationToken));
        }
    }

    private async Task SeedCompanyPricesAsync(long companyId, CancellationToken cancellationToken)
    {
        var products = await GetProductsBySeedOrderAsync(companyId, cancellationToken);

        var existingPriceList = await _dbContext.PriceLists
            .AsNoTracking()
            .FirstOrDefaultAsync(list => list.CompanyId == companyId && list.Name == "Lista General", cancellationToken);

        var priceListId = existingPriceList?.Id
            ?? EnsureSuccess(await _pricingService.CreateDefaultPriceListAsync(
                companyId,
                "Lista General",
                cancellationToken)).Id;

        var existingProductIds = await _dbContext.PriceListItems
            .AsNoTracking()
            .Where(item => item.CompanyId == companyId && item.PriceListId == priceListId)
            .Select(item => item.ProductId)
            .ToListAsync(cancellationToken);
        var existingProductSet = existingProductIds.ToHashSet();

        foreach (var priceSeed in BuildPriceSeeds(products))
        {
            if (existingProductSet.Contains(priceSeed.ProductId))
            {
                continue;
            }

            _ = EnsureSuccess(await _pricingService.AddPriceAsync(
                companyId,
                priceListId,
                priceSeed.ProductId,
                priceSeed.UnitPrice,
                cancellationToken));
        }
    }

    private async Task SeedCompanyOperationAsync(long companyId, CancellationToken cancellationToken)
    {
        var warehouseCentral = await _dbContext.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(warehouse => warehouse.CompanyId == companyId && warehouse.Code == "CENTRAL", cancellationToken)
            ?? throw new InvalidOperationException($"Warehouse CENTRAL is missing for company {companyId}. Run /api/dev/seed/base first.");
        var warehouseStore = await _dbContext.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(warehouse => warehouse.CompanyId == companyId && warehouse.Code == "TIENDA", cancellationToken)
            ?? throw new InvalidOperationException($"Warehouse TIENDA is missing for company {companyId}. Run /api/dev/seed/base first.");

        var products = await GetProductsBySeedOrderAsync(companyId, cancellationToken);
        var stockableProducts = products.Where(product => product.IsStockable).ToList();
        if (stockableProducts.Count < 6)
        {
            throw new InvalidOperationException($"Not enough stockable products for company {companyId}. Run /api/dev/seed/base first.");
        }

        var customerIds = await GetCustomerIdsBySeedOrderAsync(companyId, cancellationToken);
        if (customerIds.Count < 2)
        {
            throw new InvalidOperationException($"Not enough customers for company {companyId}. Run /api/dev/seed/base first.");
        }

        var supplierIds = await GetSupplierIdsBySeedOrderAsync(companyId, cancellationToken);
        if (supplierIds.Count == 0)
        {
            throw new InvalidOperationException($"No suppliers found for company {companyId}. Run /api/dev/seed/base first.");
        }

        var supplierId = supplierIds[0];
        var purchaseOrderOne = EnsureSuccess(await _purchasingService.CreatePurchaseOrderAsync(
            companyId,
            supplierId,
            "CLP",
            new List<(long productId, decimal qty, Money unitPriceRef, long? taxGroupId)>
            {
                (stockableProducts[0].Id, 20m, Money.CLP(180000), null),
                (stockableProducts[1].Id, 12m, Money.CLP(24000), null),
                (stockableProducts[2].Id, 15m, Money.CLP(38000), null)
            },
            cancellationToken));

        var purchaseOrderTwo = EnsureSuccess(await _purchasingService.CreatePurchaseOrderAsync(
            companyId,
            supplierId,
            "CLP",
            new List<(long productId, decimal qty, Money unitPriceRef, long? taxGroupId)>
            {
                (stockableProducts[3].Id, 10m, Money.CLP(210000), null),
                (stockableProducts[4].Id, 40m, Money.CLP(4500), null),
                (stockableProducts[5].Id, 8m, Money.CLP(92000), null)
            },
            cancellationToken));

        _ = EnsureSuccess(await _purchasingService.ReceiveGoodsAsync(
            companyId,
            supplierId,
            purchaseOrderOne.Id,
            new List<(long productId, long warehouseId, decimal receivedQty, string? batchNumber, DateTime? expiryDate)>
            {
                (stockableProducts[0].Id, warehouseCentral.Id, 20m, null, null),
                (stockableProducts[1].Id, warehouseCentral.Id, 12m, null, null),
                (stockableProducts[2].Id, warehouseStore.Id, 15m, null, null)
            },
            cancellationToken));

        _ = EnsureSuccess(await _purchasingService.ReceiveGoodsAsync(
            companyId,
            supplierId,
            purchaseOrderTwo.Id,
            new List<(long productId, long warehouseId, decimal receivedQty, string? batchNumber, DateTime? expiryDate)>
            {
                (stockableProducts[3].Id, warehouseCentral.Id, 6m, null, null),
                (stockableProducts[4].Id, warehouseCentral.Id, 25m, null, null),
                (stockableProducts[5].Id, warehouseCentral.Id, 8m, null, null)
            },
            cancellationToken));

        var salesOrderOne = EnsureSuccess(await _salesService.CreateOrderAsync(
            companyId,
            customerIds[0],
            new List<(long productId, decimal qty, Money unitPrice, long? taxGroupId)>
            {
                (stockableProducts[0].Id, 2m, Money.CLP(289000), null),
                (stockableProducts[4].Id, 4m, Money.CLP(7900), null)
            },
            cancellationToken));

        var salesOrderTwo = EnsureSuccess(await _salesService.CreateOrderAsync(
            companyId,
            customerIds[1],
            new List<(long productId, decimal qty, Money unitPrice, long? taxGroupId)>
            {
                (stockableProducts[1].Id, 3m, Money.CLP(29900), null),
                (stockableProducts[3].Id, 1m, Money.CLP(259000), null)
            },
            cancellationToken));

        _ = EnsureSuccess(await _salesService.ApproveSalesOrderAsync(
            salesOrderOne.Id,
            companyId,
            warehouseCentral.Id,
            cancellationToken));

        _ = EnsureSuccess(await _salesService.ApproveSalesOrderAsync(
            salesOrderTwo.Id,
            companyId,
            warehouseCentral.Id,
            cancellationToken));
    }

    private async Task<IReadOnlyList<Product>> GetProductsBySeedOrderAsync(long companyId, CancellationToken cancellationToken)
    {
        var productSeeds = BuildProductSeeds();
        var skus = productSeeds.Select(seed => seed.Sku).ToArray();
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.CompanyId == companyId && skus.Contains(product.Sku))
            .ToListAsync(cancellationToken);

        var productsBySku = products.ToDictionary(product => product.Sku, StringComparer.OrdinalIgnoreCase);
        var ordered = new List<Product>(productSeeds.Count);
        foreach (var seed in productSeeds)
        {
            if (!productsBySku.TryGetValue(seed.Sku, out var product))
            {
                throw new InvalidOperationException($"Product {seed.Sku} is missing for company {companyId}. Run /api/dev/seed/base first.");
            }

            ordered.Add(product);
        }

        return ordered;
    }

    private async Task<IReadOnlyList<long>> GetCustomerIdsBySeedOrderAsync(long companyId, CancellationToken cancellationToken)
    {
        var customerSeeds = BuildCustomerSeeds();
        var names = customerSeeds.Select(seed => seed.Name).ToArray();
        var customers = await _dbContext.Customers
            .AsNoTracking()
            .Where(customer => customer.CompanyId == companyId && names.Contains(customer.Name))
            .Select(customer => new { customer.Id, customer.Name })
            .ToListAsync(cancellationToken);

        var customersByName = customers.ToDictionary(customer => customer.Name, customer => customer.Id, StringComparer.OrdinalIgnoreCase);
        var ordered = new List<long>(customerSeeds.Count);
        foreach (var seed in customerSeeds)
        {
            if (!customersByName.TryGetValue(seed.Name, out var customerId))
            {
                throw new InvalidOperationException($"Customer {seed.Name} is missing for company {companyId}. Run /api/dev/seed/base first.");
            }

            ordered.Add(customerId);
        }

        return ordered;
    }

    private async Task<IReadOnlyList<long>> GetSupplierIdsBySeedOrderAsync(long companyId, CancellationToken cancellationToken)
    {
        var supplierSeeds = BuildSupplierSeeds();
        var names = supplierSeeds.Select(seed => seed.Name).ToArray();
        var suppliers = await _dbContext.Suppliers
            .AsNoTracking()
            .Where(supplier => supplier.CompanyId == companyId && names.Contains(supplier.Name))
            .Select(supplier => new { supplier.Id, supplier.Name })
            .ToListAsync(cancellationToken);

        var suppliersByName = suppliers.ToDictionary(supplier => supplier.Name, supplier => supplier.Id, StringComparer.OrdinalIgnoreCase);
        var ordered = new List<long>(supplierSeeds.Count);
        foreach (var seed in supplierSeeds)
        {
            if (!suppliersByName.TryGetValue(seed.Name, out var supplierId))
            {
                throw new InvalidOperationException($"Supplier {seed.Name} is missing for company {companyId}. Run /api/dev/seed/base first.");
            }

            ordered.Add(supplierId);
        }

        return ordered;
    }

    private async Task DeletePriceDataAsync(long companyId, CancellationToken cancellationToken)
    {
        await _dbContext.PriceListItems.Where(item => item.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.PriceLists.Where(list => list.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
    }

    private async Task DeleteOperationDataAsync(long companyId, CancellationToken cancellationToken)
    {
        var companyToken = $"\\\"companyId\\\":{companyId}";
        await _dbContext.OutboxMessages
            .Where(message => message.PayloadJson.Contains(companyToken))
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.JournalEntryLines.Where(line => line.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.JournalEntries.Where(entry => entry.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountingReceivableSchedules.Where(schedule => schedule.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountingAccountsReceivables.Where(receivable => receivable.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountingPayableSchedules.Where(schedule => schedule.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountingAccountsPayables.Where(payable => payable.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountsReceivables.Where(receivable => receivable.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountsPayables.Where(payable => payable.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.InvoiceLines.Where(line => line.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Invoices.Where(invoice => invoice.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.CreditNotes.Where(note => note.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DebitNotes.Where(note => note.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Receipts.Where(receipt => receipt.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Payments.Where(payment => payment.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.BankTransactions.Where(tx => tx.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.BankStatements.Where(statement => statement.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.BankAccounts.Where(account => account.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SalesDocumentLines.Where(line => line.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SalesDocuments.Where(document => document.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.GoodsReceiptLines.Where(line => line.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.GoodsReceipts.Where(receipt => receipt.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.PurchaseOrderLines.Where(line => line.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.PurchaseOrders.Where(order => order.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.InventoryMovements.Where(movement => movement.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Stocks.Where(stock => stock.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
    }

    private async Task DeleteCompanyDataAsync(long companyId, CancellationToken cancellationToken)
    {
        var companyToken = $"\\\"companyId\\\":{companyId}";
        await _dbContext.OutboxMessages
            .Where(message => message.PayloadJson.Contains(companyToken))
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.JournalEntryLines.Where(line => line.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.JournalEntries.Where(entry => entry.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Accounts.Where(account => account.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Journals.Where(journal => journal.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountingPeriods.Where(period => period.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.CompanyAccountingSettings.Where(settings => settings.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountingReceivableSchedules.Where(schedule => schedule.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountingAccountsReceivables.Where(receivable => receivable.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountingPayableSchedules.Where(schedule => schedule.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountingAccountsPayables.Where(payable => payable.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountsReceivables.Where(receivable => receivable.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.AccountsPayables.Where(payable => payable.CompanyId == companyId)
            .ExecuteDeleteAsync(cancellationToken);
        await _dbContext.InvoiceLines.Where(line => line.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Invoices.Where(invoice => invoice.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.CreditNotes.Where(note => note.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.DebitNotes.Where(note => note.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Receipts.Where(receipt => receipt.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Payments.Where(payment => payment.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.BankTransactions.Where(tx => tx.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.BankStatements.Where(statement => statement.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.BankAccounts.Where(account => account.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.TaxRules.Where(rule => rule.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.TaxGroups.Where(group => group.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Taxes.Where(tax => tax.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.PriceListItems.Where(item => item.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.PriceLists.Where(list => list.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SalesDocumentLines.Where(line => line.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.SalesDocuments.Where(document => document.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.GoodsReceiptLines.Where(line => line.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.GoodsReceipts.Where(receipt => receipt.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.PurchaseOrderLines.Where(line => line.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.PurchaseOrders.Where(order => order.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.InventoryMovements.Where(movement => movement.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Stocks.Where(stock => stock.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Products.Where(product => product.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Customers.Where(customer => customer.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Suppliers.Where(supplier => supplier.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.Warehouses.Where(warehouse => warehouse.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
        await _dbContext.CompanyUnitsOfMeasure.Where(unit => unit.CompanyId == companyId).ExecuteDeleteAsync(cancellationToken);
    }

    private static IReadOnlyList<ProductSeed> BuildProductSeeds() =>
    [
        new("SKU-0001", "Notebook 14 pulgadas", "780100000001", true, true, true, "EA"),
        new("SKU-0002", "Mouse inalámbrico", "780100000002", true, true, true, "EA"),
        new("SKU-0003", "Teclado mecánico", "780100000003", true, true, true, "EA"),
        new("SKU-0004", "Monitor 24 pulgadas", "780100000004", true, true, true, "EA"),
        new("SKU-0005", "Cable HDMI 2m", "780100000005", true, true, true, "EA"),
        new("SKU-0006", "Disco SSD 512GB", "780100000006", true, true, true, "EA"),
        new("SKU-0007", "Impresora térmica", "780100000007", true, true, true, "EA"),
        new("SKU-0008", "Papel A4 resma", "780100000008", true, true, true, "EA"),
        new("SKU-0009", "Tóner láser negro", "780100000009", true, true, true, "EA"),
        new("SKU-0010", "Cable eléctrico a granel", "780100000010", true, true, true, "KG"),
        new("SKU-0011", "Servicio de instalación", "780100000011", false, true, false, "EA"),
        new("SKU-0012", "Servicio de mantenimiento", "780100000012", false, true, false, "EA"),
        new("SKU-0013", "Capacitación in situ", "780100000013", false, true, false, "EA"),
        new("SKU-0014", "Diseño de etiqueta", "780100000014", false, true, false, "EA"),
        new("SKU-0015", "Transporte urbano", "780100000015", false, true, false, "EA")
    ];

    private static IReadOnlyList<PartySeed> BuildCustomerSeeds() =>
    [
        new("Minimarket Sol Azul", "99.111.111-1"),
        new("Talleres Sierra Norte", "99.222.222-2"),
        new("Distribuidora Valle Norte", "99.333.333-3"),
        new("Panadería Buen Trigo", "99.444.444-4"),
        new("Librería Estrella Sur", "99.555.555-5"),
        new("Hotel Mirador Pacífico", "99.666.666-6"),
        new("Clínica Santa Luna", "99.777.777-7"),
        new("Constructora Río Blanco", "99.888.888-8"),
        new("Agrícola Las Lomas", "99.999.999-9"),
        new("Centro Deportivo Bahía", "98.888.777-6")
    ];

    private static IReadOnlyList<PartySeed> BuildSupplierSeeds() =>
    [
        new("Distribuciones Quillay", "88.111.111-1"),
        new("Fábrica Puerto Claro", "88.222.222-2"),
        new("Logística Monte Azul", "88.333.333-3"),
        new("Tecnología Bahía Roja", "88.444.444-4"),
        new("Suministros Cerro Verde", "88.555.555-5"),
        new("Importadora Litoral Sur", "88.666.666-6")
    ];

    private static IReadOnlyList<PriceSeed> BuildPriceSeeds(IReadOnlyList<Product> products)
    {
        var priceValues = new[]
        {
            289000m, 29900m, 45000m, 259000m, 7900m,
            99000m, 189000m, 7900m, 65000m, 3500m
        };

        return products.Take(10)
            .Select((product, index) => new PriceSeed(product.Id, Money.CLP(priceValues[index])))
            .ToList();
    }

    private static T EnsureSuccess<T>(ERP.Shared.Application.Result<T> result)
    {
        if (!result.Success || result.Value is null)
        {
            throw new InvalidOperationException(result.Error ?? "Seed operation failed.");
        }

        return result.Value;
    }

    private static void EnsureSuccess(ERP.Shared.Application.Result result)
    {
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error ?? "Seed operation failed.");
        }
    }

    private sealed record DevSeedCompany(long CompanyId, string Name, string Slug, Guid CompanyPublicId);
    private sealed record ProductSeed(string Sku, string Name, string Barcode, bool IsStockable, bool IsSellable, bool IsPurchasable, string UnitCode);
    private sealed record PartySeed(string Name, string TaxId);
    private sealed record PriceSeed(long ProductId, Money UnitPrice);
}

public sealed record DevSeedCompanyResult(long CompanyId, string Name, string Slug, string Status);

public sealed record DevSeedResult(bool Seeded, bool Forced, IReadOnlyList<DevSeedCompanyResult> Companies, string Message);
