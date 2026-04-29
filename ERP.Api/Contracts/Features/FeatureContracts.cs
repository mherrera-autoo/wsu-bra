using System.ComponentModel.DataAnnotations;

namespace ERP.Api.Contracts.Features;

public sealed record FeatureChangeRequest(
    [param: Required(AllowEmptyStrings = false)] string Reason,
    [param: Required] Guid CorrelationId);

public sealed record PlatformCompanyFeatureUpsertRequest(
    [param: Required] Guid FeaturePublicId,
    [param: Required] Guid CompanyPublicId,
    bool IsActive,
    [param: Required(AllowEmptyStrings = false)] string Reason,
    [param: Required] Guid CorrelationId);

/*
Example request JSON:
{
  "reason": "Habilitacion de modulo por onboarding",
  "correlationId": "3f516f0a-f0de-4a72-b705-ef31e7f81589"
}

Example catalog response JSON:
{
  "featurePublicId": "b67457d8-1675-45a8-95f8-63cc4f4f6644",
  "code": "Pharmacy.Base",
  "name": "Farmacia base",
  "description": "Capacidades base de farmacia",
  "isActive": true,
  "isAssignedToCompany": true
}

Example list response JSON:
[
  {
    "featurePublicId": "b67457d8-1675-45a8-95f8-63cc4f4f6644",
    "code": "Pharmacy.Base",
    "name": "Farmacia base",
    "description": "Capacidades base de farmacia",
    "isActive": true,
    "isAssignedToCompany": true
  }
]
*/

public sealed record FeatureCatalogResponse(
    Guid FeaturePublicId,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    bool IsAssignedToCompany);
