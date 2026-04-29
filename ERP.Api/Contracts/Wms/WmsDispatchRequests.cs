namespace ERP.Api.Contracts.Wms;

public sealed record DispatchConfirmationRequest(
    long CompanyId,
    string ReferenceType,
    string ReferenceId,
    DateTime DeliveredAt,
    string Carrier,
    string? DriverName,
    string? VehiclePlate,
    string? ReceivedBy,
    string? Notes);
