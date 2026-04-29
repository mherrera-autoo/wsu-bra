using System.Security.Claims;
using ERP.Api.Contracts.PharmaceuticalRegulatedInventory;
using ERP.Api.Controllers;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Inventory.Application.Handlers;
using ERP.Modules.Inventory.Domain;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Modules.PharmaceuticalRegulatedInventory.Application.Handlers;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;
using ERP.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class PharmaceuticalRegulatedInventoryIntegrationTests
{
    [Fact]
    public async Task RegulatedReceipt_CreatesStockBatchAndInventoryMovement()
    {
        const decimal unitPrice = 25m;
        const decimal quantity = 4m;
        long companyId;
        Guid companyPublicId;
        long organizationId;
        var batchNumber = $"LOT-{Guid.NewGuid():N}".ToUpperInvariant()[..10];
        var expiryDate = DateTime.UtcNow.Date.AddMonths(6);
        var issuedAt = DateTime.UtcNow.Date;

        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        (companyId, companyPublicId, organizationId) = await SeedCompanyAsync(services);
        SetTenantContext(services, companyId, companyPublicId, organizationId);

        var dbContext = services.GetRequiredService<ErpDbContext>();
        var (productId, warehouseId, supplierId) = await SeedPharmacyDataAsync(dbContext, companyId);

        var controller = ActivatorUtilities.CreateInstance<PharmaceuticalRegulatedInventoryInventoryController>(services);
        var request = new PurchaseInvoiceReceiptRequest(
            companyId,
            supplierId,
            null,
            $"INV-{Guid.NewGuid():N}".ToUpperInvariant()[..12],
            issuedAt,
            issuedAt.AddDays(30),
            quantity * unitPrice,
            0m,
            quantity * unitPrice,
            "USD",
            null,
            new List<PurchaseInvoiceReceiptLineRequest>
            {
                new(
                    productId,
                    warehouseId,
                    quantity,
                    unitPrice,
                    batchNumber,
                    expiryDate,
                    InboundPresentation.Box)
            });

        var actionResult = await controller.ReceiveInvoice(request, CancellationToken.None);
        Assert.IsType<OkObjectResult>(actionResult);

        var receiptOutbox = await dbContext.OutboxMessages
            .SingleAsync(message => message.Type == "purchasing.goods-receipt.requested");
        var goodsReceiptHandler = ActivatorUtilities.CreateInstance<GoodsReceiptRequestedHandler>(services);
        await goodsReceiptHandler.HandleAsync(receiptOutbox.PayloadJson, CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        var stockBatch = await dbContext.StockBatches
            .SingleAsync(batch => batch.CompanyId == companyId
                && batch.ProductId == productId
                && batch.WarehouseId == warehouseId
                && batch.BatchNumber == batchNumber);

        var receiptPostedOutbox = await dbContext.OutboxMessages
            .SingleAsync(message => message.Type == "pharmacy.receipt.posted");
        var receiptPostedHandler = ActivatorUtilities.CreateInstance<PharmaceuticalRegulatedInventoryReceiptPostedHandler>(services);
        await receiptPostedHandler.HandleAsync(receiptPostedOutbox.PayloadJson, CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        var movement = await dbContext.InventoryMovements
            .SingleAsync(record => record.CompanyId == companyId
                && record.ProductId == productId
                && record.MovementType == MovementType.In
                && record.ReferenceId == stockBatch.PublicId.ToString());
        Assert.Equal(MovementType.In, movement.MovementType);

        var stock = await dbContext.Stocks
            .SingleAsync(record => record.CompanyId == companyId
                && record.ProductId == productId
                && record.WarehouseId == warehouseId);
        Assert.Equal(quantity, stock.OnHandQuantity);
    }

    private static async Task<(long productId, long warehouseId, long supplierId)> SeedPharmacyDataAsync(
        ErpDbContext dbContext,
        long companyId)
    {
        var unit = UnitOfMeasure.Create("EA", "EA", UnitOfMeasureDimension.Count, true, 1m, 0, true, null);
        await dbContext.UnitOfMeasures.AddAsync(unit);
        await dbContext.SaveChangesAsync();

        var product = Product.Create(companyId, "SKU-REG-001", "Regulated Product", "780000000001", unit.Id, true, true, true);
        await dbContext.Products.AddAsync(product);
        await dbContext.SaveChangesAsync();

        var warehouse = Warehouse.Create(companyId, "WH-REG", "Regulated Warehouse");
        var supplier = Supplier.Create(companyId, "Supplier");
        var profile = PharmaProductProfile.Create(
            companyId,
            product.Id,
            true,
            true,
            PharmacySaleConditions.PrescriptionRequired,
            true);
        var info = ProductPharmaInfo.Create(
            companyId,
            product.Id,
            "Lab",
            "Box",
            "Ingredient",
            false,
            null,
            "BAR-123456");
        var feature = CompanyFeature.Create(companyId, FeatureCode.PharmaceuticalBase.PublicId, true, DateTime.UtcNow);

        await dbContext.AddRangeAsync(warehouse, supplier, profile, info, feature);
        await dbContext.SaveChangesAsync();

        return (product.Id, warehouse.Id, supplier.Id);
    }

    private static async Task<(long companyId, Guid companyPublicId, long organizationId)> SeedCompanyAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ErpDbContext>();
        var organization = Organization.Create(OrganizationType.Individual, "Pharmacy Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, "Pharmacy Company");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "PHARM-ORG", "Pharmacy Company");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        return (company.Id, company.PublicId, organization.Id);
    }

    private static void SetTenantContext(IServiceProvider services, long companyId, Guid companyPublicId, long organizationId)
    {
        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(IdentityClaimTypes.Scope, "tenant"),
            new(IdentityClaimTypes.OrganizationId, organizationId.ToString()),
            new(IdentityClaimTypes.CompanyId, companyId.ToString()),
            new(IdentityClaimTypes.CompanyPublicId, companyPublicId.ToString())
        };

        var identity = new ClaimsIdentity(claims, TestAuthHandler.Scheme);
        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
    }
}
