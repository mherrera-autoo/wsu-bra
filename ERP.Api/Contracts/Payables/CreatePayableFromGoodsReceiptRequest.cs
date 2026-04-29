namespace ERP.Api.Contracts.Payables;

public sealed record CreatePayableFromGoodsReceiptRequest(
    long CompanyId,
    long GoodsReceiptId,
    DateTime? DefaultDueDate,
    IReadOnlyList<PayableScheduleRequest> Schedules);
