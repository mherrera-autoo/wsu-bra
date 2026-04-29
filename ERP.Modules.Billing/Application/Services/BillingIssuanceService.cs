using ERP.Modules.Billing.Application.Repositories;
using ERP.Modules.Billing.Domain;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Billing.Application.Services;

public sealed class BillingIssuanceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICreditNoteRepository _creditNoteRepository;
    private readonly IDebitNoteRepository _debitNoteRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly BillingNumberingService _numberingService;
    private readonly IUnitOfWork _unitOfWork;

    public BillingIssuanceService(
        IInvoiceRepository invoiceRepository,
        ICreditNoteRepository creditNoteRepository,
        IDebitNoteRepository debitNoteRepository,
        IEventPublisher eventPublisher,
        BillingNumberingService numberingService,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _creditNoteRepository = creditNoteRepository;
        _debitNoteRepository = debitNoteRepository;
        _eventPublisher = eventPublisher;
        _numberingService = numberingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Invoice>> IssueInvoiceAsync(
        long companyId,
        long customerId,
        long receivableAccountId,
        long revenueAccountId,
        IEnumerable<(long productId, decimal qty, Money unitPrice)> lines,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var invoice = Invoice.Create(companyId, customerId);
            foreach (var (productId, qty, unitPrice) in lines)
            {
                invoice.AddLine(productId, qty, unitPrice);
            }

            var (total, currencyResult) = CalculateInvoiceTotal(invoice);
            if (!currencyResult.Success)
            {
                return Result<Invoice>.Fail(currencyResult.Error ?? "Invalid invoice currency.");
            }

            var number = await _numberingService.GetNextInvoiceNumberAsync(companyId, token);
            invoice.Issue(number, DateTime.UtcNow);

            await _invoiceRepository.AddAsync(invoice, token);
            return Result<Invoice>.Ok(invoice);
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        var invoiceResult = result.Value!;
        var currency = invoiceResult.Lines.First().UnitPrice.Currency;
        var totalAmount = invoiceResult.Lines.Sum(line => line.Qty * line.UnitPrice.Amount);

        return result;
    }

    public async Task<Result<Invoice>> CancelInvoiceAsync(
        long invoiceId,
        long companyId,
        long receivableAccountId,
        long revenueAccountId,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, token);
            if (invoice is null || invoice.CompanyId != companyId)
            {
                return Result<Invoice>.Fail("Invoice not found.");
            }

            var (total, currencyResult) = CalculateInvoiceTotal(invoice);
            if (!currencyResult.Success)
            {
                return Result<Invoice>.Fail(currencyResult.Error ?? "Invalid invoice currency.");
            }

            invoice.Cancel();
            return Result<Invoice>.Ok(invoice);
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        var invoiceResult = result.Value!;
        var currency = invoiceResult.Lines.First().UnitPrice.Currency;
        var totalAmount = invoiceResult.Lines.Sum(line => line.Qty * line.UnitPrice.Amount);

        return result;
    }

    public async Task<Result<CreditNote>> IssueCreditNoteAsync(
        long companyId,
        long invoiceId,
        Money amount,
        long receivableAccountId,
        long revenueAccountId,
        CancellationToken cancellationToken = default)
    {
        if (amount.Amount <= 0)
        {
            return Result<CreditNote>.Fail("Amount must be greater than zero.");
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var creditNote = CreditNote.Create(companyId, invoiceId, amount, "Manual credit note");
            var number = await _numberingService.GetNextCreditNoteNumberAsync(companyId, token);
            creditNote.Issue(number, DateTime.UtcNow);

            await _creditNoteRepository.AddAsync(creditNote, token);
            return Result<CreditNote>.Ok(creditNote);
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        var creditResult = result.Value!;
        return result;
    }

    public async Task<Result<CreditNote>> CancelCreditNoteAsync(
        long creditNoteId,
        long companyId,
        long receivableAccountId,
        long revenueAccountId,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var creditNote = await _creditNoteRepository.GetByIdAsync(creditNoteId, token);
            if (creditNote is null || creditNote.CompanyId != companyId)
            {
                return Result<CreditNote>.Fail("Credit note not found.");
            }

            creditNote.Cancel();
            return Result<CreditNote>.Ok(creditNote);
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        var creditResult = result.Value!;
        return result;
    }

    public async Task<Result<DebitNote>> IssueDebitNoteAsync(
        long companyId,
        long invoiceId,
        Money amount,
        long receivableAccountId,
        long revenueAccountId,
        CancellationToken cancellationToken = default)
    {
        if (amount.Amount <= 0)
        {
            return Result<DebitNote>.Fail("Amount must be greater than zero.");
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var debitNote = DebitNote.Create(companyId, invoiceId, amount);
            var number = await _numberingService.GetNextDebitNoteNumberAsync(companyId, token);
            debitNote.Issue(number, DateTime.UtcNow);

            await _debitNoteRepository.AddAsync(debitNote, token);
            return Result<DebitNote>.Ok(debitNote);
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        var debitResult = result.Value!;
        return result;
    }

    public async Task<Result<DebitNote>> CancelDebitNoteAsync(
        long debitNoteId,
        long companyId,
        long receivableAccountId,
        long revenueAccountId,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var debitNote = await _debitNoteRepository.GetByIdAsync(debitNoteId, token);
            if (debitNote is null || debitNote.CompanyId != companyId)
            {
                return Result<DebitNote>.Fail("Debit note not found.");
            }

            debitNote.Cancel();
            return Result<DebitNote>.Ok(debitNote);
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        var debitResult = result.Value!;
        return result;
    }

    private static (decimal total, Result currencyResult) CalculateInvoiceTotal(Invoice invoice)
    {
        if (!invoice.Lines.Any())
        {
            return (0, Result.Fail("Invoice has no lines."));
        }

        var currency = invoice.Lines.First().UnitPrice.Currency;
        foreach (var line in invoice.Lines)
        {
            if (!string.Equals(line.UnitPrice.Currency, currency, StringComparison.OrdinalIgnoreCase))
            {
                return (0, Result.Fail("Invoice lines must use the same currency."));
            }
        }

        var total = invoice.Lines.Sum(line => line.Qty * line.UnitPrice.Amount);
        if (total <= 0)
        {
            return (0, Result.Fail("Invoice total must be greater than zero."));
        }

        return (total, Result.Ok());
    }
}
