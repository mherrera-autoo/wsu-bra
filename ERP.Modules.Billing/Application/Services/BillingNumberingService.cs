using ERP.Modules.Billing.Application.Repositories;

namespace ERP.Modules.Billing.Application.Services;

public sealed class BillingNumberingService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly ICreditNoteRepository _creditNoteRepository;
    private readonly IDebitNoteRepository _debitNoteRepository;

    public BillingNumberingService(
        IInvoiceRepository invoiceRepository,
        ICreditNoteRepository creditNoteRepository,
        IDebitNoteRepository debitNoteRepository)
    {
        _invoiceRepository = invoiceRepository;
        _creditNoteRepository = creditNoteRepository;
        _debitNoteRepository = debitNoteRepository;
    }

    public async Task<long> GetNextInvoiceNumberAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var maxNumber = await _invoiceRepository.GetMaxNumberAsync(companyId, cancellationToken);
        return (maxNumber ?? 0) + 1;
    }

    public async Task<long> GetNextCreditNoteNumberAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var maxNumber = await _creditNoteRepository.GetMaxNumberAsync(companyId, cancellationToken);
        return (maxNumber ?? 0) + 1;
    }

    public async Task<long> GetNextDebitNoteNumberAsync(long companyId, CancellationToken cancellationToken = default)
    {
        var maxNumber = await _debitNoteRepository.GetMaxNumberAsync(companyId, cancellationToken);
        return (maxNumber ?? 0) + 1;
    }
}
