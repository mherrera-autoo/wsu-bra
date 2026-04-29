using System;

namespace ERP.Modules.Sales.Contracts;

public sealed record SalesReceivableRequested(
    long CompanyId,
    long DocumentId,
    long CustomerId,
    string DocumentKind,
    decimal TotalAmount,
    string Currency,
    DateTime IssueDate,
    DateTime? DueDate);
