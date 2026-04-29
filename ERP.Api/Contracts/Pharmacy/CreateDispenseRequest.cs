using System.Text.Json.Serialization;
using ERP.Modules.PharmaceuticalRegulatedInventory.Domain;

namespace ERP.Api.Contracts.Pharmacy;

// TODO: Move dispensing DTOs to ERP.Api.Contracts.RetailPharmacy (ERP.Modules.RetailPharmacy).
public sealed record CreateDispenseRequest(
    long CompanyId,
    DateTime Date,
    long PerformedByUserId,
    IReadOnlyList<CreateDispenseLineRequest> Lines,
    CreatePrescriptionRequest? Prescription);

public sealed record CreateDispenseLineRequest(
    long ProductId,
    decimal Quantity);

public sealed record CreatePrescriptionRequest(
    [property: JsonConverter(typeof(JsonStringEnumConverter))] PrescriptionType Type,
    DateTime IssuedAt,
    DateTime? ExpiresAt,
    string Patient,
    string? PatientIdentification,
    string Doctor,
    string? DoctorIdentification,
    string Folio,
    string? AttachmentUrl);
