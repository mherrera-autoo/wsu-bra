using System.Collections.Generic;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Sales.Contracts;

public enum SalesDocumentKind
{
    Quote = 1,
    Order = 2,
    Invoice = 3,
    CreditNote = 4
}

public enum SalesDocumentStatus
{
    Draft = 1,
    Approved = 2,
    Sent = 3,
    Cancelled = 4
}

public sealed record SalesDocumentSnapshot(
    long Id,
    long CompanyId,
    long CustomerId,
    SalesDocumentKind Kind,
    SalesDocumentStatus Status,
    IReadOnlyList<SalesDocumentLineSnapshot> Lines);

public sealed record SalesDocumentLineSnapshot(
    long ProductId,
    decimal Qty,
    Money UnitPrice);
