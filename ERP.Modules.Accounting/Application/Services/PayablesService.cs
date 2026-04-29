using ERP.Modules.Accounting.Application.Repositories;
using ERP.Modules.Accounting.Domain;
using ERP.Modules.Purchasing.Contracts;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Accounting.Application.Services;

public sealed class PayablesService
{
    private readonly IAccountsPayableRepository _accountsPayableRepository;
    private readonly IPurchaseOrderQuery _purchaseOrderQuery;
    private readonly IGoodsReceiptQuery _goodsReceiptQuery;
    private readonly IUnitOfWork _unitOfWork;

    public PayablesService(
        IAccountsPayableRepository accountsPayableRepository,
        IPurchaseOrderQuery purchaseOrderQuery,
        IGoodsReceiptQuery goodsReceiptQuery,
        IUnitOfWork unitOfWork)
    {
        _accountsPayableRepository = accountsPayableRepository;
        _purchaseOrderQuery = purchaseOrderQuery;
        _goodsReceiptQuery = goodsReceiptQuery;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AccountsPayable>> CreateFromPurchaseOrderAsync(
        long companyId,
        long purchaseOrderId,
        DateTime? defaultDueDate,
        IEnumerable<(DateTime dueDate, decimal amount)> schedules,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _purchaseOrderQuery.GetByIdAsync(purchaseOrderId, cancellationToken);
        if (purchaseOrder is null || purchaseOrder.CompanyId != companyId)
        {
            return Result<AccountsPayable>.Fail("Purchase order not found.");
        }

        if (purchaseOrder.Lines.Count == 0)
        {
            return Result<AccountsPayable>.Fail("Purchase order has no lines.");
        }

        var currency = purchaseOrder.Lines.First().UnitPriceRef.Currency;
        if (purchaseOrder.Lines.Any(line => !string.Equals(line.UnitPriceRef.Currency, currency, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<AccountsPayable>.Fail("Purchase order has mixed currencies.");
        }

        var total = purchaseOrder.Lines.Sum(line => line.OrderedQty * line.UnitPriceRef.Amount);
        var totalAmount = new Money(total, currency);

        var payable = AccountsPayable.Create(
            purchaseOrder.CompanyId,
            purchaseOrder.SupplierId,
            PayableSourceType.PurchaseOrder,
            purchaseOrder.Id,
            totalAmount);

        var scheduleList = schedules?.ToList() ?? new List<(DateTime dueDate, decimal amount)>();
        if (scheduleList.Count == 0)
        {
            var dueDate = defaultDueDate ?? DateTime.UtcNow.AddDays(30);
            payable.AddSchedule(dueDate, totalAmount);
        }
        else
        {
            var scheduleTotal = scheduleList.Sum(s => s.amount);
            if (scheduleTotal != totalAmount.Amount)
            {
                return Result<AccountsPayable>.Fail("Schedule amounts must match total payable.");
            }

            foreach (var schedule in scheduleList)
            {
                payable.AddSchedule(schedule.dueDate, new Money(schedule.amount, currency));
            }
        }

        payable.RefreshStatus();

        await _accountsPayableRepository.AddAsync(payable, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AccountsPayable>.Ok(payable);
    }

    public async Task<Result<AccountsPayable>> CreateFromGoodsReceiptAsync(
        long companyId,
        long goodsReceiptId,
        DateTime? defaultDueDate,
        IEnumerable<(DateTime dueDate, decimal amount)> schedules,
        CancellationToken cancellationToken = default)
    {
        var receipt = await _goodsReceiptQuery.GetByIdAsync(companyId, goodsReceiptId, cancellationToken);
        if (receipt is null || receipt.CompanyId != companyId)
        {
            return Result<AccountsPayable>.Fail("Goods receipt not found.");
        }

        if (receipt.PurchaseOrderId is null)
        {
            return Result<AccountsPayable>.Fail("Goods receipt must be linked to a purchase order.");
        }

        var purchaseOrder = await _purchaseOrderQuery.GetByIdAsync(receipt.PurchaseOrderId.Value, cancellationToken);
        if (purchaseOrder is null)
        {
            return Result<AccountsPayable>.Fail("Purchase order not found for goods receipt.");
        }

        var currency = purchaseOrder.Lines.FirstOrDefault()?.UnitPriceRef.Currency;
        if (currency is null)
        {
            return Result<AccountsPayable>.Fail("Purchase order has no pricing information.");
        }

        var priceLookup = purchaseOrder.Lines
            .GroupBy(line => line.ProductId)
            .ToDictionary(group => group.Key, group => group.First().UnitPriceRef);

        decimal total = 0m;
        foreach (var line in receipt.Lines)
        {
            if (!priceLookup.TryGetValue(line.ProductId, out var unitPrice))
            {
                return Result<AccountsPayable>.Fail($"Missing pricing for product {line.ProductId}.");
            }

            if (!string.Equals(unitPrice.Currency, currency, StringComparison.OrdinalIgnoreCase))
            {
                return Result<AccountsPayable>.Fail("Purchase order has mixed currencies.");
            }

            total += line.ReceivedQty * unitPrice.Amount;
        }

        if (total <= 0)
        {
            return Result<AccountsPayable>.Fail("Goods receipt has no payable amount.");
        }

        var totalAmount = new Money(total, currency);

        var payable = AccountsPayable.Create(
            receipt.CompanyId,
            receipt.SupplierId,
            PayableSourceType.GoodsReceipt,
            receipt.Id,
            totalAmount);

        var scheduleList = schedules?.ToList() ?? new List<(DateTime dueDate, decimal amount)>();
        if (scheduleList.Count == 0)
        {
            var dueDate = defaultDueDate ?? DateTime.UtcNow.AddDays(30);
            payable.AddSchedule(dueDate, totalAmount);
        }
        else
        {
            var scheduleTotal = scheduleList.Sum(s => s.amount);
            if (scheduleTotal != totalAmount.Amount)
            {
                return Result<AccountsPayable>.Fail("Schedule amounts must match total payable.");
            }

            foreach (var schedule in scheduleList)
            {
                payable.AddSchedule(schedule.dueDate, new Money(schedule.amount, currency));
            }
        }

        payable.RefreshStatus();

        await _accountsPayableRepository.AddAsync(payable, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AccountsPayable>.Ok(payable);
    }

    public Task<AccountsPayable?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _accountsPayableRepository.GetByIdAsync(id, cancellationToken);
}
