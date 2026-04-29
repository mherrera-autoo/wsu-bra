using System.ComponentModel.DataAnnotations;

namespace ERP.Api.Contracts.Companies;

public sealed record CompanyFeatureToggleRequest(
    [property: Required(AllowEmptyStrings = false)] string Reason,
    [property: Required] Guid CorrelationId);
