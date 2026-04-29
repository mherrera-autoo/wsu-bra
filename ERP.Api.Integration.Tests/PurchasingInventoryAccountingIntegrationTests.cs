using System.Security.Claims;
using ERP.Modules.Accounting.Application.Services;
using ERP.Modules.Identity.Application.Security;
using ERP.Modules.Inventory.Application.Handlers;
using ERP.Modules.Purchasing.Application.Services;
using ERP.Persistence;
using ERP.Shared.Application;
using ERP.Modules.Accounting.Contracts;
using ERP.Modules.MasterData.Contracts;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ERP.Api.Integration.Tests;

public sealed class PurchasingInventoryAccountingIntegrationTests
{
    [Fact]
    public async Task GoodsReceipt_WiresInventoryOutboxAndAccountingEntries()
    {
        const long supplierId = 901;
        const long productId = 42;
        const long warehouseId = 3;
        const decimal unitPrice = 25m;
        const decimal qty = 4m;
        long companyId;
        Guid companyPublicId;

        using var factory = new ApiWebApplicationFactory(Guid.NewGuid().ToString());
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        (companyId, companyPublicId) = await SeedCompanyAndCurrencyAsync(services);
        SetTenantContext(services, companyId, companyPublicId);

        var bootstrapService = services.GetRequiredService<AccountingBootstrapService>();
        var seedResult = await bootstrapService.EnsureSeededAsync(companyId, "USD", CancellationToken.None);
        Assert.True(seedResult.Success, seedResult.Error);

        var purchasingService = services.GetRequiredService<PurchasingService>();
        var purchaseOrderResult = await purchasingService.CreatePurchaseOrderAsync(
            companyId,
            supplierId,
            "USD",
            new[]
            {
                (productId, qty, new Money(unitPrice, "USD"), (long?)null)
            },
            CancellationToken.None);
        Assert.True(purchaseOrderResult.Success, purchaseOrderResult.Error);

        var receiptResult = await purchasingService.ReceiveGoodsAsync(
            companyId,
            supplierId,
            purchaseOrderResult.Value!.Id,
            new (long productId, long warehouseId, decimal receivedQty, string? batchNumber, DateTime? expiryDate)[]
            {
                (productId, warehouseId, qty, "LOT-001", DateTime.UtcNow.Date.AddMonths(6))
            },
            CancellationToken.None);
        Assert.True(receiptResult.Success, receiptResult.Error);

        var dbContext = services.GetRequiredService<ErpDbContext>();
        var receipt = receiptResult.Value!;

        var receiptOutbox = await dbContext.OutboxMessages
            .SingleAsync(message => message.Type == "purchasing.goods-receipt.requested");
        var receiptHandler = ActivatorUtilities.CreateInstance<GoodsReceiptRequestedHandler>(services);
        await receiptHandler.HandleAsync(receiptOutbox.PayloadJson, CancellationToken.None);
        dbContext.ChangeTracker.Clear();

        var movements = await dbContext.InventoryMovements
            .Where(movement => movement.CompanyId == companyId
                && movement.ReferenceType == "GoodsReceipt"
                && movement.ReferenceId == receipt.Id.ToString())
            .ToListAsync();
        Assert.Single(movements);

        var stock = await dbContext.Stocks.SingleAsync(s =>
            s.CompanyId == companyId
            && s.ProductId == productId
            && s.WarehouseId == warehouseId);
        Assert.Equal(qty, stock.OnHandQuantity);

        var inventoryAccountId = await dbContext.Accounts
            .Where(account => account.CompanyId == companyId && account.Code == "1.1.3")
            .Select(account => account.Id)
            .SingleAsync();

        var payablesAccountId = await dbContext.Accounts
            .Where(account => account.CompanyId == companyId && account.Code == "2.1")
            .Select(account => account.Id)
            .SingleAsync();

        var eventPublisher = services.GetRequiredService<IEventPublisher>();
        var totalAmount = qty * unitPrice;

        await eventPublisher.PublishAsync(new AccountingPostRequested(
            CorrelationId: $"gr-{receipt.Id}",
            SourceModule: "Inventory",
            SourceDocumentType: "GoodsReceipt",
            SourceDocumentId: receipt.Id.ToString(),
            JournalCode: "PURCHASES",
            EntryDate: DateTime.UtcNow.Date,
            Description: "Goods receipt posting",
            Lines: new List<AccountingPostRequestedLine>
            {
                new(inventoryAccountId, totalAmount, 0m, null, null),
                new(payablesAccountId, 0m, totalAmount, null, null)
            }),
            CancellationToken.None);

        var entry = await dbContext.JournalEntries
            .Include(journalEntry => journalEntry.Lines)
            .SingleAsync(journalEntry => journalEntry.CompanyId == companyId
                && journalEntry.SourceModule == "Inventory"
                && journalEntry.SourceDocumentType == "GoodsReceipt"
                && journalEntry.SourceDocumentId == receipt.Id.ToString());

        Assert.Equal(2, entry.Lines.Count);

        var debitLine = entry.Lines.Single(line => line.AccountId == inventoryAccountId);
        var creditLine = entry.Lines.Single(line => line.AccountId == payablesAccountId);

        Assert.Equal(totalAmount, debitLine.Debit);
        Assert.Equal(0m, debitLine.Credit);
        Assert.Equal(0m, creditLine.Debit);
        Assert.Equal(totalAmount, creditLine.Credit);

        var outboxTypes = await dbContext.OutboxMessages
            .Select(message => message.Type)
            .ToListAsync();
        Assert.Contains("inventory.goods-received", outboxTypes);
        Assert.Contains("accounting.journal-entry.posted", outboxTypes);
    }

    private static void SetTenantContext(IServiceProvider services, long companyId, Guid companyPublicId)
    {
        var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(IdentityClaimTypes.Scope, "tenant"),
            new(IdentityClaimTypes.OrganizationId, "1"),
            new(IdentityClaimTypes.CompanyId, companyId.ToString()),
            new(IdentityClaimTypes.CompanyPublicId, companyPublicId.ToString())
        };

        var identity = new ClaimsIdentity(claims, TestAuthHandler.Scheme);
        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
    }

    private static async Task<(long companyId, Guid companyPublicId)> SeedCompanyAndCurrencyAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ErpDbContext>();
        var organization = Organization.Create(OrganizationType.Individual, "Purchasing Org");
        await dbContext.Organizations.AddAsync(organization);
        await dbContext.SaveChangesAsync();

        var company = Company.Create(organization.Id, 0, "Purchasing Company");
        await dbContext.Companies.AddAsync(company);
        await dbContext.SaveChangesAsync();

        var taxEntity = TaxEntity.Create(company.PublicId, "PURCH-ORG", "Purchasing Co");
        await dbContext.TaxEntities.AddAsync(taxEntity);
        await dbContext.SaveChangesAsync();

        company.UpdateTaxEntityId(taxEntity.Id);
        await dbContext.SaveChangesAsync();

        var currency = Currency.Create("USD", 840, "US Dollar", "$", 2, 1, true);
        await dbContext.Currencies.AddAsync(currency);
        await dbContext.SaveChangesAsync();

        await dbContext.CompanyCurrencies.AddAsync(CompanyCurrency.Create(company.Id, currency.Id, isDefault: true, isActive: true));
        await dbContext.SaveChangesAsync();

        return (company.Id, company.PublicId);
    }
}
