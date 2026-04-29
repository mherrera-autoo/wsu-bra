using ERP.Modules.Cash.Application.Repositories;
using ERP.Modules.Cash.Domain;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Cash.Application.Services;

public sealed class CashApplicationService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReceiptRepository _receiptRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;

    public CashApplicationService(
        IPaymentRepository paymentRepository,
        IReceiptRepository receiptRepository,
        IBankAccountRepository bankAccountRepository,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork)
    {
        _paymentRepository = paymentRepository;
        _receiptRepository = receiptRepository;
        _bankAccountRepository = bankAccountRepository;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<Payment>> RecordPaymentAsync(
        long companyId,
        long supplierId,
        long bankAccountId,
        Money amount,
        DateTime paidAt,
        long payableAccountId,
        long bankLedgerAccountId,
        string? reference,
        CancellationToken cancellationToken = default)
    {
        return RecordPaymentInternalAsync(
            companyId,
            supplierId,
            bankAccountId,
            amount,
            paidAt,
            payableAccountId,
            bankLedgerAccountId,
            reference,
            cancellationToken);
    }

    public Task<Result<Receipt>> RecordReceiptAsync(
        long companyId,
        long customerId,
        long bankAccountId,
        Money amount,
        DateTime receivedAt,
        long receivableAccountId,
        long bankLedgerAccountId,
        string? reference,
        CancellationToken cancellationToken = default)
    {
        return RecordReceiptInternalAsync(
            companyId,
            customerId,
            bankAccountId,
            amount,
            receivedAt,
            receivableAccountId,
            bankLedgerAccountId,
            reference,
            cancellationToken);
    }

    private async Task<Result<Payment>> RecordPaymentInternalAsync(
        long companyId,
        long supplierId,
        long bankAccountId,
        Money amount,
        DateTime paidAt,
        long payableAccountId,
        long bankLedgerAccountId,
        string? reference,
        CancellationToken cancellationToken)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId, token);
            if (bankAccount is null || bankAccount.CompanyId != companyId)
            {
                return Result<Payment>.Fail("Bank account not found.");
            }

            var payment = Payment.Create(companyId, supplierId, bankAccountId, amount, paidAt, reference);
            payment.Post();

            await _paymentRepository.AddAsync(payment, token);

            return Result<Payment>.Ok(payment);
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        var paymentResult = result.Value!;
        return result;
    }

    private async Task<Result<Receipt>> RecordReceiptInternalAsync(
        long companyId,
        long customerId,
        long bankAccountId,
        Money amount,
        DateTime receivedAt,
        long receivableAccountId,
        long bankLedgerAccountId,
        string? reference,
        CancellationToken cancellationToken)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId, token);
            if (bankAccount is null || bankAccount.CompanyId != companyId)
            {
                return Result<Receipt>.Fail("Bank account not found.");
            }

            var receipt = Receipt.Create(companyId, customerId, bankAccountId, amount, receivedAt, reference);
            receipt.Post();

            await _receiptRepository.AddAsync(receipt, token);

            return Result<Receipt>.Ok(receipt);
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        var receiptResult = result.Value!;
        return result;
    }
}
