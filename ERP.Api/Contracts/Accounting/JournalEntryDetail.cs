namespace ERP.Api.Contracts.Accounting;

public sealed record JournalEntryLineDetail(long AccountId, decimal Debit, decimal Credit);

public sealed record JournalEntryDetail(
    long Id,
    string? Reference,
    DateTime EntryDate,
    IReadOnlyList<JournalEntryLineDetail> Lines);
