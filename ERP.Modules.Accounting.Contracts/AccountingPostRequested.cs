using System;
using System.Collections.Generic;

namespace ERP.Modules.Accounting.Contracts;

public sealed record AccountingPostRequested(
    string CorrelationId,
    string SourceModule,
    string SourceDocumentType,
    string SourceDocumentId,
    string JournalCode,
    DateTime EntryDate,
    string? Description,
    IReadOnlyList<AccountingPostRequestedLine> Lines);

public sealed record AccountingPostRequestedLine(
    long AccountId,
    decimal Debit,
    decimal Credit,
    string? CurrencyCode,
    decimal? FxRate);
