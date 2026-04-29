using System;
using System.Reflection;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.MasterData.Domain;
using ERP.Modules.Purchasing.Application.Repositories;
using ERP.Modules.Purchasing.Application.Services;
using ERP.Modules.Purchasing.Domain;
using ERP.Modules.Tax.Contracts;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;
using Xunit;

namespace ERP.Modules.Purchasing.WhiteBox.Tests;

public sealed class PurchasingSuggestionsTests
{
    [Fact]
    public async Task GenerateSuggestions_UsesStockoutAndHistory()
    {
        var suggestionRepository = new InMemoryPurchaseSuggestionRepository();
        var dataQuery = new InMemorySuggestionDataQuery(
            new[] { new StockSnapshot(10, 5, 0m) },
            new[]
            {
                new PurchaseHistorySnapshot(10, 7, 5m, DateTime.UtcNow.AddDays(-10)),
                new PurchaseHistorySnapshot(10, 7, 10m, DateTime.UtcNow.AddDays(-20)),
                new PurchaseHistorySnapshot(10, 7, 15m, DateTime.UtcNow.AddDays(-30))
            });

        var service = BuildService(
            suggestionRepository,
            dataQuery);

        var result = await service.GeneratePurchaseSuggestionsAsync(
            1,
            new PurchaseSuggestionGenerateInput(null, null, Array.Empty<PurchaseSuggestionMinimumInput>()));

        Assert.True(result.Success, result.Error);
        var suggestion = result.Value;
        Assert.NotNull(suggestion);
        Assert.Single(suggestion!.Lines);

        var line = suggestion.Lines.Single();
        Assert.Equal(PurchaseSuggestionReason.Stockout, line.Reason);
        Assert.Equal(10m, line.QtySuggested);
        Assert.Equal(7, line.SupplierSuggestedId);
    }

    [Fact]
    public async Task ApproveAndConvertSuggestion_CreatesPurchaseOrder()
    {
        var suggestionRepository = new InMemoryPurchaseSuggestionRepository();
        var purchaseOrderRepository = new InMemoryPurchaseOrderRepository();
        var service = BuildService(suggestionRepository, new InMemorySuggestionDataQuery(Array.Empty<StockSnapshot>(), Array.Empty<PurchaseHistorySnapshot>()), purchaseOrderRepository);

        var createResult = await service.CreatePurchaseSuggestionAsync(
            1,
            new PurchaseSuggestionCreateInput(
                SupplierSuggestedId: 99,
                new[]
                {
                    new PurchaseSuggestionLineInput(50, 12m, 99, PurchaseSuggestionReason.Manual, 3)
                }));

        Assert.True(createResult.Success, createResult.Error);

        var approveResult = await service.ApprovePurchaseSuggestionAsync(1, createResult.Value!.Id);
        Assert.True(approveResult.Success, approveResult.Error);

        var convertResult = await service.ConvertPurchaseSuggestionToPurchaseOrderAsync(
            1,
            createResult.Value.Id,
            new PurchaseSuggestionConvertInput(null, "CLP"));

        Assert.True(convertResult.Success, convertResult.Error);
        Assert.Single(purchaseOrderRepository.PurchaseOrders);

        var suggestion = await suggestionRepository.GetByIdAsync(1, createResult.Value.Id);
        Assert.NotNull(suggestion);
        Assert.Equal(PurchaseSuggestionStatus.Converted, suggestion!.Status);
        Assert.NotNull(suggestion.ConvertedPurchaseOrderId);
    }

    private static PurchasingService BuildService(
        IPurchaseSuggestionRepository suggestionRepository,
        IPurchaseSuggestionDataQuery dataQuery,
        IPurchaseOrderRepository? purchaseOrderRepository = null)
    {
        purchaseOrderRepository ??= new InMemoryPurchaseOrderRepository();
        var currency = Currency.Create("CLP", 152, "Chilean Peso", "$", 0, 1);
        SetEntityId(currency, 1);
        var currencyRepository = new InMemoryCurrencyRepository(new[] { currency });
        return new PurchasingService(
            purchaseOrderRepository,
            new InMemoryGoodsReceiptRepository(),
            suggestionRepository,
            dataQuery,
            new FakeTaxCalculationQuery(),
            new FakeCompanyCurrencyValidationService(1),
            currencyRepository,
            new InMemoryUnitOfWork(),
            new InMemoryOutboxRepository());
    }

    private sealed class InMemoryPurchaseSuggestionRepository : IPurchaseSuggestionRepository
    {
        private readonly List<PurchaseSuggestion> _suggestions = new();
        private long _nextId = 1;

        public Task AddAsync(PurchaseSuggestion suggestion, CancellationToken cancellationToken = default)
        {
            SetEntityId(suggestion, _nextId++);
            foreach (var line in suggestion.Lines)
            {
                SetEntityId(line, _nextId++);
            }

            _suggestions.Add(suggestion);
            return Task.CompletedTask;
        }

        public Task<PurchaseSuggestion?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
            => Task.FromResult(_suggestions.FirstOrDefault(suggestion => suggestion.CompanyId == companyId && suggestion.Id == id));

        public Task<IReadOnlyList<PurchaseSuggestion>> ListAsync(long companyId, long? warehouseId, long? supplierId, PurchaseSuggestionStatus? status, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PurchaseSuggestion>>(_suggestions);
    }

    private sealed class InMemorySuggestionDataQuery : IPurchaseSuggestionDataQuery
    {
        private readonly IReadOnlyList<StockSnapshot> _stocks;
        private readonly IReadOnlyList<PurchaseHistorySnapshot> _history;

        public InMemorySuggestionDataQuery(IReadOnlyList<StockSnapshot> stocks, IReadOnlyList<PurchaseHistorySnapshot> history)
        {
            _stocks = stocks;
            _history = history;
        }

        public Task<IReadOnlyList<StockSnapshot>> ListStocksAsync(long companyId, long? warehouseId, CancellationToken cancellationToken = default)
            => Task.FromResult(_stocks);

        public Task<IReadOnlyList<PurchaseHistorySnapshot>> ListRecentPurchaseLinesAsync(long companyId, DateTime since, long? supplierId, CancellationToken cancellationToken = default)
            => Task.FromResult(_history);
    }

    private sealed class InMemoryPurchaseOrderRepository : IPurchaseOrderRepository
    {
        private long _nextId = 1;
        public List<PurchaseOrder> PurchaseOrders { get; } = new();

        public Task AddAsync(PurchaseOrder purchaseOrder, CancellationToken cancellationToken = default)
        {
            SetEntityId(purchaseOrder, _nextId++);
            foreach (var line in purchaseOrder.Lines)
            {
                SetEntityId(line, _nextId++);
            }

            PurchaseOrders.Add(purchaseOrder);
            return Task.CompletedTask;
        }

        public Task<PurchaseOrder?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => Task.FromResult<PurchaseOrder?>(PurchaseOrders.FirstOrDefault(order => order.Id == id));

        public Task<PurchaseOrder?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default)
            => Task.FromResult<PurchaseOrder?>(PurchaseOrders.FirstOrDefault(order => order.CompanyId == companyId && order.Id == id));

        public Task<IReadOnlyList<PurchaseOrder>> ListAsync(long companyId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<PurchaseOrder>>(PurchaseOrders);
    }

    private sealed class InMemoryGoodsReceiptRepository : IGoodsReceiptRepository
    {
        public Task AddAsync(GoodsReceipt receipt, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<GoodsReceipt?> GetByIdAsync(long companyId, long id, CancellationToken cancellationToken = default) => Task.FromResult<GoodsReceipt?>(null);
        public Task<IReadOnlyList<GoodsReceipt>> ListAsync(long companyId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<GoodsReceipt>>(Array.Empty<GoodsReceipt>());
    }

    private sealed class FakeCompanyCurrencyValidationService : ICompanyCurrencyValidationService
    {
        private readonly long _defaultCurrencyId;

        public FakeCompanyCurrencyValidationService(long defaultCurrencyId)
        {
            _defaultCurrencyId = defaultCurrencyId;
        }

        public Task<Result> ValidateCurrencyForCompanyAsync(long companyId, long currencyId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Ok());

        public Task<Result<IEnumerable<long>>> GetAllowedCurrencyIdsForCompanyAsync(long companyId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<IEnumerable<long>>.Ok(new[] { _defaultCurrencyId }));

        public Task<Result<long?>> GetDefaultCurrencyIdForCompanyAsync(long companyId, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<long?>.Ok(_defaultCurrencyId));

        public Task<bool> IsCurrencyAllowedForCompanyAsync(long companyId, long currencyId, CancellationToken cancellationToken = default)
            => Task.FromResult(true);
    }

    private sealed class InMemoryCurrencyRepository : ICurrencyRepository
    {
        private readonly List<Currency> _currencies;

        public InMemoryCurrencyRepository(IEnumerable<Currency> currencies)
        {
            _currencies = currencies.ToList();
        }

        public Task AddAsync(Currency currency, CancellationToken cancellationToken = default)
        {
            _currencies.Add(currency);
            return Task.CompletedTask;
        }

        public Task AddRangeAsync(IEnumerable<Currency> currencies, CancellationToken cancellationToken = default)
        {
            _currencies.AddRange(currencies);
            return Task.CompletedTask;
        }

        public Task<Currency?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => Task.FromResult(_currencies.FirstOrDefault(currency => currency.Id == id));

        public Task<Currency?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
            => Task.FromResult(_currencies.FirstOrDefault(currency => string.Equals(currency.Code, code, StringComparison.OrdinalIgnoreCase)));

        public Task<bool> ExistsByCodeAsync(string code, long? excludeId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(_currencies.Any(currency =>
                string.Equals(currency.Code, code, StringComparison.OrdinalIgnoreCase)
                && (!excludeId.HasValue || currency.Id != excludeId.Value)));

        public Task<IReadOnlyList<Currency>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Currency>>(_currencies);

        public Task<int> CountAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_currencies.Count);
    }

    private sealed class FakeTaxCalculationQuery : ITaxCalculationQuery
    {
        public Task<Result<Money>> CalculateTaxAmountAsync(long companyId, long? taxGroupId, decimal baseAmount, string currency, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<Money>.Ok(new Money(0, currency)));
    }

    private sealed class InMemoryUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

        public Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> action, CancellationToken cancellationToken = default)
            => action(cancellationToken);

        public Task<Result<T>> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<Result<T>>> action, CancellationToken cancellationToken = default)
            => action(cancellationToken);
    }

    private sealed class InMemoryOutboxRepository : IOutboxRepository
    {
        public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(
            int batchSize,
            DateTime utcNow,
            int maxAttempts,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<OutboxMessage>>(Array.Empty<OutboxMessage>());
        public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private static void SetEntityId(object entity, long id)
    {
        var property = entity.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property?.SetValue(entity, id);
    }
}
