namespace ERP.Api.Contracts.Accounting;

public sealed record JournalEntrySummary(long Id, string? Reference, DateTime EntryDate);
