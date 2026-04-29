using ERP.Modules.Cash.Application.Repositories;
using ERP.Modules.Cash.Domain;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Cash.Application.Services;

public sealed class BankReconciliationService
{
    private readonly IBankStatementRepository _bankStatementRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BankReconciliationService(
        IBankStatementRepository bankStatementRepository,
        IBankAccountRepository bankAccountRepository,
        IPaymentRepository paymentRepository,
        IReceiptRepository receiptRepository,
        IUnitOfWork unitOfWork)
    {
        _bankStatementRepository = bankStatementRepository;
        _bankAccountRepository = bankAccountRepository;
        _paymentRepository = paymentRepository;
        _receiptRepository = receiptRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<BankStatement>> CreateStatementAsync(
        long companyId,
        long bankAccountId,
        DateTime statementDate,
        decimal startingBalance,
        decimal endingBalance,
        IEnumerable<BankStatementTransactionInfo> transactions,
        CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var bankAccount = await _bankAccountRepository.GetByIdAsync(bankAccountId, token);
            if (bankAccount is null || bankAccount.CompanyId != companyId)
            {
                return Result<BankStatement>.Fail("Bank account not found.");
            }

            var statement = BankStatement.Create(companyId, bankAccountId, statementDate, startingBalance, endingBalance);
            foreach (var transaction in transactions)
            {
                var kind = transaction.Amount >= 0 ? BankTransactionKind.Credit : BankTransactionKind.Debit;
                var amount = new Money(Math.Abs(transaction.Amount), bankAccount.Currency);
                statement.AddTransaction(
                    amount,
                    kind,
                    transaction.TransactionDate,
                    transaction.Description,
                    transaction.Reference);
            }

            await _bankStatementRepository.AddAsync(statement, token);
            return Result<BankStatement>.Ok(statement);
        }, cancellationToken);
    }

    public Task<Result<BankStatement>> ReconcileStatementAsync(
        long companyId,
        long statementId,
        IEnumerable<BankTransactionMatch> matches,
        CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var statement = await _bankStatementRepository.GetByIdAsync(statementId, token);
            if (statement is null || statement.CompanyId != companyId)
            {
                return Result<BankStatement>.Fail("Bank statement not found.");
            }

            foreach (var match in matches)
            {
                var transaction = statement.Transactions.FirstOrDefault(t => t.Id == match.BankTransactionId);
                if (transaction is null)
                {
                    return Result<BankStatement>.Fail($"Bank transaction {match.BankTransactionId} not found.");
                }

                if (match.PaymentId is null && match.ReceiptId is null)
                {
                    return Result<BankStatement>.Fail("PaymentId or ReceiptId is required for reconciliation.");
                }

                if (match.PaymentId is not null && match.ReceiptId is not null)
                {
                    return Result<BankStatement>.Fail("Only one of PaymentId or ReceiptId can be used.");
                }

                if (match.PaymentId is not null)
                {
                    var payment = await _paymentRepository.GetByIdAsync(match.PaymentId.Value, token);
                    if (payment is null || payment.CompanyId != companyId)
                    {
                        return Result<BankStatement>.Fail("Payment not found.");
                    }

                    transaction.MatchPayment(payment.Id);
                }
                else if (match.ReceiptId is not null)
                {
                    var receipt = await _receiptRepository.GetByIdAsync(match.ReceiptId.Value, token);
                    if (receipt is null || receipt.CompanyId != companyId)
                    {
                        return Result<BankStatement>.Fail("Receipt not found.");
                    }

                    transaction.MatchReceipt(receipt.Id);
                }
            }

            return Result<BankStatement>.Ok(statement);
        }, cancellationToken);
    }
}

public readonly record struct BankStatementTransactionInfo(
    DateTime TransactionDate,
    decimal Amount,
    string Description,
    string? Reference);

public readonly record struct BankTransactionMatch(
    long BankTransactionId,
    long? PaymentId,
    long? ReceiptId);
