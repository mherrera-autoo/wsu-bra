using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.Purchasing.Contracts;
using ERP.Modules.Purchasing.Application.Repositories;
using ERP.Modules.Purchasing.Domain;
using ERP.Modules.Tax.Contracts;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Purchasing.Application.Services;

public sealed class PurchasingService
{
    private const string MissingCurrencyMessage = "Currency is required.";
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IGoodsReceiptRepository _goodsReceiptRepository;
    private readonly IPurchaseSuggestionRepository _purchaseSuggestionRepository;
    private readonly IPurchaseSuggestionDataQuery _purchaseSuggestionDataQuery;
    private readonly ITaxCalculationQuery _taxCalculationQuery;
    private readonly ICompanyCurrencyValidationService _companyCurrencyValidationService;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outboxRepository;

    public PurchasingService(
        IPurchaseOrderRepository purchaseOrderRepository,
        IGoodsReceiptRepository goodsReceiptRepository,
        IPurchaseSuggestionRepository purchaseSuggestionRepository,
        IPurchaseSuggestionDataQuery purchaseSuggestionDataQuery,
        ITaxCalculationQuery taxCalculationQuery,
        ICompanyCurrencyValidationService companyCurrencyValidationService,
        ICurrencyRepository currencyRepository,
        IUnitOfWork unitOfWork,
        IOutboxRepository outboxRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _goodsReceiptRepository = goodsReceiptRepository;
        _purchaseSuggestionRepository = purchaseSuggestionRepository;
        _purchaseSuggestionDataQuery = purchaseSuggestionDataQuery;
        _taxCalculationQuery = taxCalculationQuery;
        _companyCurrencyValidationService = companyCurrencyValidationService;
        _currencyRepository = currencyRepository;
        _unitOfWork = unitOfWork;
        _outboxRepository = outboxRepository;
    }

    public async Task<Result<PurchaseOrder>> CreatePurchaseOrderAsync(
        long companyId,
        long supplierId,
        string currency,
        IEnumerable<(long productId, decimal qty, Money unitPriceRef, long? taxGroupId)> lines,
        CancellationToken cancellationToken = default)
    {
        var lineItems = lines.ToList();
        var currencyResolution = await ResolveCurrencyAsync(companyId, currency, cancellationToken);
        if (!currencyResolution.Success)
        {
            return Result<PurchaseOrder>.Fail(currencyResolution.Error ?? MissingCurrencyMessage);
        }

        var orderCurrency = currencyResolution.Value!;
        if (lineItems.Any(line => !string.Equals(line.unitPriceRef.Currency, orderCurrency, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<PurchaseOrder>.Fail("Unit price currency must match purchase order currency.");
        }

        var purchaseOrder = PurchaseOrder.Create(companyId, supplierId, orderCurrency);
        foreach (var (productId, qty, unitPriceRef, taxGroupId) in lineItems)
        {
            var baseAmount = qty * unitPriceRef.Amount;
            var taxResult = await _taxCalculationQuery.CalculateTaxAmountAsync(companyId, taxGroupId, baseAmount, unitPriceRef.Currency, cancellationToken);
            if (!taxResult.Success)
            {
                return Result<PurchaseOrder>.Fail(taxResult.Error ?? "Failed to calculate taxes.");
            }

            purchaseOrder.AddLine(productId, qty, unitPriceRef, taxGroupId, taxResult.Value);
        }

        await _purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PurchaseOrder>.Ok(purchaseOrder);
    }

    public async Task<Result<GoodsReceipt>> ReceiveGoodsAsync(
        long companyId,
        long supplierId,
        long? purchaseOrderId,
        IEnumerable<(long productId, long warehouseId, decimal receivedQty, string? batchNumber, DateTime? expiryDate)> lines,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var receipt = GoodsReceipt.Create(companyId, supplierId, purchaseOrderId);
            foreach (var (productId, warehouseId, receivedQty, _, _) in lines)
            {
                receipt.AddLine(productId, warehouseId, receivedQty);
            }

            await _goodsReceiptRepository.AddAsync(receipt, token);
            await _unitOfWork.SaveChangesAsync(token);

            var receiptEvent = new GoodsReceiptRequested(
                receipt.CompanyId,
                receipt.Id,
                receipt.SupplierId,
                receipt.PurchaseOrderId,
                lines.Select(line => new ERP.Modules.Purchasing.Contracts.GoodsReceiptLine(
                        line.productId,
                        line.warehouseId,
                        line.receivedQty,
                        line.batchNumber,
                        line.expiryDate))
                    .ToList());
            var payload = JsonSerializer.Serialize(receiptEvent);
            await _outboxRepository.AddAsync(
                OutboxMessage.Create("purchasing.goods-receipt.requested", payload),
                token);

            return Result<GoodsReceipt>.Ok(receipt);
        }, cancellationToken);
    }

    public Task<PurchaseOrder?> GetPurchaseOrderAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _purchaseOrderRepository.GetByIdAsync(companyId, id, cancellationToken);

    public Task<GoodsReceipt?> GetGoodsReceiptAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _goodsReceiptRepository.GetByIdAsync(companyId, id, cancellationToken);

    public Task<IReadOnlyList<PurchaseOrder>> ListPurchaseOrdersAsync(long companyId, CancellationToken cancellationToken = default)
        => _purchaseOrderRepository.ListAsync(companyId, cancellationToken);

    public Task<IReadOnlyList<GoodsReceipt>> ListGoodsReceiptsAsync(long companyId, CancellationToken cancellationToken = default)
        => _goodsReceiptRepository.ListAsync(companyId, cancellationToken);

    public Task<IReadOnlyList<PurchaseSuggestion>> ListPurchaseSuggestionsAsync(
        long companyId,
        long? warehouseId,
        long? supplierId,
        PurchaseSuggestionStatus? status,
        CancellationToken cancellationToken = default)
        => _purchaseSuggestionRepository.ListAsync(companyId, warehouseId, supplierId, status, cancellationToken);

    public async Task<Result<PurchaseSuggestion>> CreatePurchaseSuggestionAsync(
        long companyId,
        PurchaseSuggestionCreateInput input,
        CancellationToken cancellationToken = default)
    {
        if (input.Lines.Count == 0)
        {
            return Result<PurchaseSuggestion>.Fail("Suggestion must include at least one line.");
        }

        var suggestion = PurchaseSuggestion.Create(companyId, input.SupplierSuggestedId);
        foreach (var line in input.Lines)
        {
            suggestion.AddLine(
                line.ProductId,
                line.QtySuggested,
                line.SupplierSuggestedId,
                line.Reason,
                line.WarehouseId);
        }

        await _purchaseSuggestionRepository.AddAsync(suggestion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PurchaseSuggestion>.Ok(suggestion);
    }

    public async Task<Result<PurchaseSuggestion>> UpdatePurchaseSuggestionAsync(
        long companyId,
        long suggestionId,
        PurchaseSuggestionUpdateInput input,
        CancellationToken cancellationToken = default)
    {
        var suggestion = await _purchaseSuggestionRepository.GetByIdAsync(companyId, suggestionId, cancellationToken);
        if (suggestion is null)
        {
            return Result<PurchaseSuggestion>.Fail("Purchase suggestion not found.");
        }

        if (input.Lines.Count == 0)
        {
            return Result<PurchaseSuggestion>.Fail("Suggestion must include at least one line.");
        }

        suggestion.ReplaceLines(input.Lines);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PurchaseSuggestion>.Ok(suggestion);
    }

    public async Task<Result> ApprovePurchaseSuggestionAsync(long companyId, long suggestionId, CancellationToken cancellationToken = default)
    {
        var suggestion = await _purchaseSuggestionRepository.GetByIdAsync(companyId, suggestionId, cancellationToken);
        if (suggestion is null)
        {
            return Result.Fail("Purchase suggestion not found.");
        }

        suggestion.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> RejectPurchaseSuggestionAsync(long companyId, long suggestionId, CancellationToken cancellationToken = default)
    {
        var suggestion = await _purchaseSuggestionRepository.GetByIdAsync(companyId, suggestionId, cancellationToken);
        if (suggestion is null)
        {
            return Result.Fail("Purchase suggestion not found.");
        }

        suggestion.Reject();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public Task<PurchaseSuggestion?> GetPurchaseSuggestionAsync(
        long companyId,
        long suggestionId,
        CancellationToken cancellationToken = default)
        => _purchaseSuggestionRepository.GetByIdAsync(companyId, suggestionId, cancellationToken);

    public Task<Result<PurchaseOrder>> ConvertPurchaseSuggestionToPurchaseOrderAsync(
        long companyId,
        long suggestionId,
        PurchaseSuggestionConvertInput input,
        CancellationToken cancellationToken = default)
        => _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var suggestion = await _purchaseSuggestionRepository.GetByIdAsync(companyId, suggestionId, token);
            if (suggestion is null)
            {
                return Result<PurchaseOrder>.Fail("Purchase suggestion not found.");
            }

            if (suggestion.Status != PurchaseSuggestionStatus.Approved)
            {
                return Result<PurchaseOrder>.Fail("Only approved suggestions can be converted.");
            }

            if (suggestion.Lines.Count == 0)
            {
                return Result<PurchaseOrder>.Fail("Suggestion has no lines to convert.");
            }

            var supplierId = input.SupplierId ?? suggestion.SupplierSuggestedId ?? suggestion.Lines.Select(line => line.SupplierSuggestedId).FirstOrDefault(id => id.HasValue);
            if (!supplierId.HasValue)
            {
                return Result<PurchaseOrder>.Fail("Supplier is required to convert to purchase order.");
            }

            var currencyResolution = await ResolveCurrencyAsync(companyId, input.Currency, token);
            if (!currencyResolution.Success)
            {
                return Result<PurchaseOrder>.Fail(currencyResolution.Error ?? MissingCurrencyMessage);
            }

            var orderCurrency = currencyResolution.Value!;
            var purchaseOrder = PurchaseOrder.Create(companyId, supplierId.Value, orderCurrency);
            foreach (var line in suggestion.Lines)
            {
                var unitPriceRef = new Money(0, orderCurrency);
                var taxResult = await _taxCalculationQuery.CalculateTaxAmountAsync(companyId, null, 0, orderCurrency, token);
                if (!taxResult.Success)
                {
                    return Result<PurchaseOrder>.Fail(taxResult.Error ?? "Failed to calculate taxes.");
                }

                purchaseOrder.AddLine(line.ProductId, line.QtySuggested, unitPriceRef, null, taxResult.Value);
            }

            await _purchaseOrderRepository.AddAsync(purchaseOrder, token);
            await _unitOfWork.SaveChangesAsync(token);
            suggestion.MarkConverted(purchaseOrder.Id);
            await _unitOfWork.SaveChangesAsync(token);
            return Result<PurchaseOrder>.Ok(purchaseOrder);
        }, cancellationToken);

    private async Task<Result<string>> ResolveCurrencyAsync(long companyId, string? currency, CancellationToken cancellationToken)
    {
        var currencyCode = string.IsNullOrWhiteSpace(currency) ? null : currency.Trim();
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            var defaultCurrencyResult = await _companyCurrencyValidationService.GetDefaultCurrencyIdForCompanyAsync(companyId, cancellationToken);
            if (!defaultCurrencyResult.Success)
            {
                return Result<string>.Fail(defaultCurrencyResult.Error ?? MissingCurrencyMessage);
            }

            if (!defaultCurrencyResult.Value.HasValue)
            {
                return Result<string>.Fail("Base currency is required.");
            }

            var defaultCurrency = await _currencyRepository.GetByIdAsync(defaultCurrencyResult.Value.Value, cancellationToken);
            if (defaultCurrency is null)
            {
                return Result<string>.Fail("Base currency not found.");
            }

            currencyCode = defaultCurrency.Code;
        }

        var currencyEntity = await _currencyRepository.GetByCodeAsync(currencyCode, cancellationToken);
        if (currencyEntity is null)
        {
            return Result<string>.Fail($"Currency code '{currencyCode}' was not found.");
        }

        var validationResult = await _companyCurrencyValidationService.ValidateCurrencyForCompanyAsync(companyId, currencyEntity.Id, cancellationToken);
        if (!validationResult.Success)
        {
            return Result<string>.Fail(validationResult.Error ?? "Currency is not allowed for the company.");
        }

        return Result<string>.Ok(currencyEntity.Code);
    }

    public async Task<Result<PurchaseSuggestion?>> GeneratePurchaseSuggestionsAsync(
        long companyId,
        PurchaseSuggestionGenerateInput input,
        CancellationToken cancellationToken = default)
    {
        var stocks = await _purchaseSuggestionDataQuery.ListStocksAsync(companyId, input.WarehouseId, cancellationToken);
        if (stocks.Count == 0)
        {
            return Result<PurchaseSuggestion?>.Ok(null);
        }

        var minimumLookup = input.Minimums
            .GroupBy(min => (min.ProductId, min.WarehouseId))
            .ToDictionary(group => group.Key, group => group.OrderByDescending(item => item.MinimumQty).First().MinimumQty);

        var historySince = DateTime.UtcNow.AddMonths(-6);
        var historyLines = await _purchaseSuggestionDataQuery.ListRecentPurchaseLinesAsync(companyId, historySince, input.SupplierId, cancellationToken);
        var historyLookup = historyLines
            .OrderByDescending(line => line.CreatedAt)
            .GroupBy(line => line.ProductId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var suggestion = PurchaseSuggestion.Create(companyId, input.SupplierId);
        foreach (var stock in stocks)
        {
            var minKey = (stock.ProductId, (long?)stock.WarehouseId);
            var defaultKey = (stock.ProductId, (long?)null);
            var minimum = minimumLookup.TryGetValue(minKey, out var specificMin)
                ? specificMin
                : minimumLookup.TryGetValue(defaultKey, out var defaultMin)
                    ? defaultMin
                    : 0m;

            if (stock.OnHandQuantity > minimum)
            {
                continue;
            }

            var reason = stock.OnHandQuantity <= 0 ? PurchaseSuggestionReason.Stockout : PurchaseSuggestionReason.Minimum;
            var suggestedSupplier = input.SupplierId;
            decimal suggestedQty = 0m;

            if (historyLookup.TryGetValue(stock.ProductId, out var history))
            {
                var recentLines = history.Take(3).ToList();
                suggestedQty = recentLines.Average(line => line.OrderedQty);
                if (!suggestedSupplier.HasValue)
                {
                    suggestedSupplier = recentLines.First().SupplierId;
                }
            }

            if (suggestedQty <= 0)
            {
                suggestedQty = Math.Max(1m, minimum - stock.OnHandQuantity);
            }

            suggestion.AddLine(stock.ProductId, suggestedQty, suggestedSupplier, reason, stock.WarehouseId);
        }

        if (suggestion.Lines.Count == 0)
        {
            return Result<PurchaseSuggestion?>.Ok(null);
        }

        await _purchaseSuggestionRepository.AddAsync(suggestion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PurchaseSuggestion?>.Ok(suggestion);
    }
}
