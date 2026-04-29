namespace ERP.Api.Contracts.Finance;

public sealed record CreateAccountsReceivableRequest(
    long CompanyId,
    long SalesDocumentId,
    DateTime? DueDate);
