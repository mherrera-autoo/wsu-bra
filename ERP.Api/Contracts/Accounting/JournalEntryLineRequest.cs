namespace ERP.Api.Contracts.Accounting;

public sealed record JournalEntryLineRequest(long AccountId, decimal Debit, decimal Credit);
