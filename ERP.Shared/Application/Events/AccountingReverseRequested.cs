namespace ERP.Shared.Application.Events;

public sealed record AccountingReverseRequested(
    string CorrelationId,
    long OriginalJournalEntryId,
    string? Description);
