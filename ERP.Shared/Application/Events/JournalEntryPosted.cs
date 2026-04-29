namespace ERP.Shared.Application.Events;

public sealed record JournalEntryPosted(
    long JournalEntryId,
    string SourceModule,
    string SourceDocumentType,
    string SourceDocumentId,
    DateTime PostingDate,
    DateTime EntryDate,
    long PeriodId,
    decimal TotalDebitBase,
    decimal TotalCreditBase,
    string CorrelationId);
