namespace ERP.Api.Contracts.Payables;

public sealed record CreatePayableFromPurchaseOrderRequest(
    long CompanyId,
    long PurchaseOrderId,
    DateTime? DefaultDueDate,
    IReadOnlyList<PayableScheduleRequest> Schedules);
