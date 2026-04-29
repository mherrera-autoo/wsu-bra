using ERP.Modules.Cash.Application.Repositories;
using ERP.Modules.Cash.Domain;
using ERP.Modules.Purchasing.Contracts;
using ERP.Modules.Sales.Contracts;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Cash.Application.Services;

public sealed class CashService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReceiptRepository _receiptRepository;
    private readonly IBankStatementRepository _bankStatementRepository;
    private readonly IBankTransactionRepository _bankTransactionRepository;
    private readonly ISalesDocumentQuery _salesDocumentQuery;
    private readonly IPurchaseOrderQuery _purchaseOrderQuery;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;

    public CashService(
        IPaymentRepository paymentRepository,
        IReceiptRepository receiptRepository,
        IBankStatementRepository bankStatementRepository,
        IBankTransactionRepository bankTransactionRepository,
        ISalesDocumentQuery salesDocumentQuery,
        IPurchaseOrderQuery purchaseOrderQuery,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _receiptRepository = receiptRepository;
        _bankStatementRepository = bankStatementRepository;
        _bankTransactionRepository = bankTransactionRepository;
        _salesDocumentQuery = salesDocumentQuery;
        _purchaseOrderQuery = purchaseOrderQuery;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Receipt>> CreateReceiptAsync(
        long companyId,
        long customerId,
        long? salesDocumentId,
        Money amount,
        DateTime? receivedAt,
        string? reference,
        CancellationToken cancellationToken = default)
    {
        var receipt = Receipt.Create(companyId, customerId, salesDocumentId, amount, receivedAt, reference);
        await _receiptRepository.AddAsync(receipt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Receipt>.Ok(receipt);
    }

    public async Task<Result<Payment>> CreatePaymentAsync(
        long companyId,
        long supplierId,
        long? purchaseOrderId,
        Money amount,
        DateTime? paidAt,
        string? reference,
        CancellationToken cancellationToken = default)
    {
        var payment = Payment.Create(companyId, supplierId, purchaseOrderId, amount, paidAt, reference);
        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Payment>.Ok(payment);
    }

    public async Task<Result<BankStatement>> CreateBankStatementAsync(
        long companyId,
        string bankAccountName,
        DateTime? statementDate,
        CancellationToken cancellationToken = default)
    {
        var statement = BankStatement.Create(companyId, bankAccountName, statementDate);
        await _bankStatementRepository.AddAsync(statement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<BankStatement>.Ok(statement);
    }

    public async Task<Result<BankTransaction>> AddBankTransactionAsync(
        long companyId,
        long bankStatementId,
        Money amount,
        BankTransactionKind kind,
        DateTime? transactionDate,
        string? description,
        string? reference,
        CancellationToken cancellationToken = default)
    {
        var statement = await _bankStatementRepository.GetByIdAsync(bankStatementId, cancellationToken);
        if (statement is null || statement.CompanyId != companyId)
        {
            return Result<BankTransaction>.Fail("Bank statement not found.");
        }

        var transaction = BankTransaction.Create(companyId, bankStatementId, amount, kind, transactionDate, description, reference);
        await _bankTransactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<BankTransaction>.Ok(transaction);
    }

    public async Task<Result> ApplyReceiptAsync(
        long companyId,
        long receiptId,
        long salesDocumentId,
        decimal amount,
        long bankAccountId,
        long accountsReceivableAccountId,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var receipt = await _receiptRepository.GetByIdAsync(receiptId, token);
            if (receipt is null || receipt.CompanyId != companyId)
            {
                return Result.Fail("Receipt not found.");
            }

            var document = await _salesDocumentQuery.GetByIdAsync(companyId, salesDocumentId, token);
            if (document is null || document.CompanyId != companyId)
            {
                return Result.Fail("Sales document not found.");
            }

            if (document.Kind != SalesDocumentKind.Invoice)
            {
                return Result.Fail("Only invoices can receive receipts.");
            }

            if (receipt.SalesDocumentId.HasValue && receipt.SalesDocumentId != salesDocumentId)
            {
                return Result.Fail("Receipt is linked to another sales document.");
            }

            receipt.Apply(amount);
            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        return result;
    }

    public async Task<Result> ApplyPaymentAsync(
        long companyId,
        long paymentId,
        long purchaseOrderId,
        decimal amount,
        long bankAccountId,
        long accountsPayableAccountId,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, token);
            if (payment is null || payment.CompanyId != companyId)
            {
                return Result.Fail("Payment not found.");
            }

            var order = await _purchaseOrderQuery.GetByIdAsync(companyId, purchaseOrderId, token);
            if (order is null || order.CompanyId != companyId)
            {
                return Result.Fail("Purchase order not found.");
            }

            if (payment.PurchaseOrderId.HasValue && payment.PurchaseOrderId != purchaseOrderId)
            {
                return Result.Fail("Payment is linked to another purchase order.");
            }

            payment.Apply(amount);
            return Result.Ok();
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        return result;
    }

    public async Task<Result> ReconcileBankTransactionAsync(
        long companyId,
        long bankTransactionId,
        long? receiptId,
        long? paymentId,
        CancellationToken cancellationToken = default)
    {
        if (receiptId is null && paymentId is null)
        {
            return Result.Fail("ReceiptId or PaymentId is required for reconciliation.");
        }

        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var transaction = await _bankTransactionRepository.GetByIdAsync(bankTransactionId, token);
            if (transaction is null || transaction.CompanyId != companyId)
            {
                return Result.Fail("Bank transaction not found.");
            }

            if (receiptId.HasValue)
            {
                var receipt = await _receiptRepository.GetByIdAsync(receiptId.Value, token);
                if (receipt is null || receipt.CompanyId != companyId)
                {
                    return Result.Fail("Receipt not found.");
                }

                transaction.MatchReceipt(receipt.Id);
                return Result.Ok();
            }

            var payment = await _paymentRepository.GetByIdAsync(paymentId!.Value, token);
            if (payment is null || payment.CompanyId != companyId)
            {
                return Result.Fail("Payment not found.");
            }

            transaction.MatchPayment(payment.Id);
            return Result.Ok();
        }, cancellationToken);
    }
}
