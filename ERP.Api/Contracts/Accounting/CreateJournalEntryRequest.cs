namespace ERP.Api.Contracts.Accounting;

public sealed record CreateJournalEntryRequest(string? Reference, IReadOnlyList<JournalEntryLineRequest> Lines);
