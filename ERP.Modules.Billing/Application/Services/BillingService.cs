using ERP.Modules.Billing.Application.Repositories;
using ERP.Modules.Billing.Domain;
using ERP.Shared.Application;
using ERP.Modules.Accounting.Contracts;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Billing.Application.Services;

public sealed class BillingService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICreditNoteRepository _creditNoteRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IUnitOfWork _unitOfWork;

    public BillingService(
        IInvoiceRepository invoiceRepository,
        ICreditNoteRepository creditNoteRepository,
        IEventPublisher eventPublisher,
        IUnitOfWork unitOfWork)
    {
        _invoiceRepository = invoiceRepository;
        _creditNoteRepository = creditNoteRepository;
        _eventPublisher = eventPublisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Invoice>> IssueInvoiceAsync(
        long companyId,
        long customerId,
        IEnumerable<(long productId, decimal qty, Money unitPrice)> lines,
        long receivableAccountId,
        long revenueAccountId,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var invoice = Invoice.Create(companyId, customerId);
            foreach (var (productId, qty, unitPrice) in lines)
            {
                invoice.AddLine(productId, qty, unitPrice);
            }

            invoice.Issue(DateTime.UtcNow);
            await _invoiceRepository.AddAsync(invoice, token);

            var total = invoice.GetTotal();
            if (total.Amount <= 0)
            {
                return Result<Invoice>.Fail("Invoice total must be greater than zero.");
            }

            return Result<Invoice>.Ok(invoice);
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        var invoiceResult = result.Value!;
        var total = invoiceResult.GetTotal();
        await _eventPublisher.PublishAsync(
            new AccountingPostRequested(
                Guid.NewGuid().ToString(),
                "Billing",
                "Invoice",
                invoiceResult.Id.ToString(),
                "SALES",
                invoiceResult.IssueDate ?? DateTime.UtcNow,
                "Invoice issuance",
                new List<AccountingPostRequestedLine>
                {
                    new(receivableAccountId, total.Amount, 0m, total.Currency, null),
                    new(revenueAccountId, 0m, total.Amount, total.Currency, null)
                }),
            cancellationToken);

        return result;
    }

    public async Task<Result<(Invoice invoice, CreditNote creditNote)>> CancelInvoiceAsync(
        long invoiceId,
        long companyId,
        string reason,
        long receivableAccountId,
        long revenueAccountId,
        CancellationToken cancellationToken = default)
    {
        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, token);
            if (invoice is null || invoice.CompanyId != companyId)
            {
                return Result<(Invoice, CreditNote)>.Fail("Invoice not found.");
            }

            if (invoice.Status != InvoiceStatus.Issued)
            {
                return Result<(Invoice, CreditNote)>.Fail("Only issued invoices can be cancelled.");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                return Result<(Invoice, CreditNote)>.Fail("Cancellation reason is required.");
            }

            var total = invoice.GetTotal();
            invoice.Cancel();

            var creditNote = CreditNote.Create(companyId, invoice.Id, total, reason);
            await _creditNoteRepository.AddAsync(creditNote, token);

            return Result<(Invoice, CreditNote)>.Ok((invoice, creditNote));
        }, cancellationToken);

        if (!result.Success)
        {
            return result;
        }

        var invoiceResult = result.Value!.Item1;
        var total = invoiceResult.GetTotal();
        await _eventPublisher.PublishAsync(
            new AccountingPostRequested(
                Guid.NewGuid().ToString(),
                "Billing",
                "InvoiceCancellation",
                invoiceResult.Id.ToString(),
                "SALES",
                DateTime.UtcNow,
                "Invoice cancellation",
                new List<AccountingPostRequestedLine>
                {
                    new(revenueAccountId, total.Amount, 0m, total.Currency, null),
                    new(receivableAccountId, 0m, total.Amount, total.Currency, null)
                }),
            cancellationToken);

        return result;
    }
}
