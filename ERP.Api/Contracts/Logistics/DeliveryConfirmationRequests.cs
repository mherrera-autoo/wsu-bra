namespace ERP.Api.Contracts.Logistics;

public sealed record DeliveryConfirmationRequest(
    long CompanyId,
    string ReferenceType,
    string ReferenceId,
    DateTime DeliveredAt,
    string? Carrier,
    string? ReceivedBy,
    string? Notes);
