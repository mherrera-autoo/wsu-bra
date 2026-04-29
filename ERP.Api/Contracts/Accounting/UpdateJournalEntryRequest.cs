namespace ERP.Api.Contracts.Accounting;

public sealed record UpdateJournalEntryRequest(string? Reference, IReadOnlyList<JournalEntryLineRequest> Lines);
