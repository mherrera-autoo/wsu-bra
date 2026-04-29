using ERP.Modules.Billing.Application.Repositories;
using ERP.Modules.Billing.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Billing.Application.Services;

public sealed class BillingStatusService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICreditNoteRepository _creditNoteRepository;
    private readonly IDebitNoteRepository _debitNoteRepository;

    public BillingStatusService(
        IInvoiceRepository invoiceRepository,
        ICreditNoteRepository creditNoteRepository,
        IDebitNoteRepository debitNoteRepository)
    {
        _invoiceRepository = invoiceRepository;
        _creditNoteRepository = creditNoteRepository;
        _debitNoteRepository = debitNoteRepository;
    }

    public async Task<Result<BillingDocumentStatus>> GetInvoiceStatusAsync(
        long invoiceId,
        long companyId,
        CancellationToken cancellationToken = default)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice is null || invoice.CompanyId != companyId)
        {
            return Result<BillingDocumentStatus>.Fail("Invoice not found.");
        }

        return Result<BillingDocumentStatus>.Ok(MapInvoiceStatus(invoice.Status));
    }

    public async Task<Result<BillingDocumentStatus>> GetCreditNoteStatusAsync(
        long creditNoteId,
        long companyId,
        CancellationToken cancellationToken = default)
    {
        var creditNote = await _creditNoteRepository.GetByIdAsync(creditNoteId, cancellationToken);
        if (creditNote is null || creditNote.CompanyId != companyId)
        {
            return Result<BillingDocumentStatus>.Fail("Credit note not found.");
        }

        return Result<BillingDocumentStatus>.Ok(creditNote.Status);
    }

    public async Task<Result<BillingDocumentStatus>> GetDebitNoteStatusAsync(
        long debitNoteId,
        long companyId,
        CancellationToken cancellationToken = default)
    {
        var debitNote = await _debitNoteRepository.GetByIdAsync(debitNoteId, cancellationToken);
        if (debitNote is null || debitNote.CompanyId != companyId)
        {
            return Result<BillingDocumentStatus>.Fail("Debit note not found.");
        }

        return Result<BillingDocumentStatus>.Ok(debitNote.Status);
    }

    private static BillingDocumentStatus MapInvoiceStatus(InvoiceStatus status)
        => status switch
        {
            InvoiceStatus.Draft => BillingDocumentStatus.Draft,
            InvoiceStatus.Issued => BillingDocumentStatus.Issued,
            InvoiceStatus.Cancelled => BillingDocumentStatus.Cancelled,
            _ => BillingDocumentStatus.Draft
        };
}
