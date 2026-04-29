using ERP.Shared.Domain;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Cash.Domain;

public sealed class BankStatement : CompanyEntity
{
    public long BankAccountId { get; private set; }
    public decimal StartingBalance { get; private set; }
    public decimal EndingBalance { get; private set; }
    public string BankAccountName { get; private set; } = string.Empty;
    public DateTime StatementDate { get; private set; } = DateTime.UtcNow;
    public List<BankTransaction> Transactions { get; private set; } = new();

    private BankStatement() { }

    public static BankStatement Create(
        long companyId,
        long bankAccountId,
        DateTime statementDate,
        decimal startingBalance,
        decimal endingBalance)
        => new()
        {
            CompanyId = companyId,
            BankAccountId = bankAccountId,
            StatementDate = statementDate,
            StartingBalance = startingBalance,
            EndingBalance = endingBalance
        };

    public static BankStatement Create(long companyId, string bankAccountName, DateTime? statementDate = null)
        => new()
        {
            CompanyId = companyId,
            BankAccountName = bankAccountName.Trim(),
            StatementDate = statementDate ?? DateTime.UtcNow
        };

    public void AddTransaction(
        Money amount,
        BankTransactionKind kind,
        DateTime transactionDate,
        string description,
        string? reference)
    {
        Transactions.Add(BankTransaction.Create(CompanyId, Id, amount, kind, transactionDate, description, reference));
    }

    public void AddTransaction(BankTransaction transaction)
    {
        if (transaction.CompanyId != CompanyId)
        {
            throw new InvalidOperationException("Transaction company does not match statement.");
        }

        Transactions.Add(transaction);
    }
}
