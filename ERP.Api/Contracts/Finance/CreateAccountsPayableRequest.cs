namespace ERP.Api.Contracts.Finance;

public sealed record CreateAccountsPayableRequest(
    long CompanyId,
    long PurchaseOrderId,
    DateTime? DueDate);
