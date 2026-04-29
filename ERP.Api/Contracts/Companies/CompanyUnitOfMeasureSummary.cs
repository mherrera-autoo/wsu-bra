using System.Text.Json.Serialization;
using ERP.Modules.MasterData.Domain;

namespace ERP.Api.Contracts.Companies;

public sealed record CompanyUnitOfMeasureSummary(
    long UnitOfMeasureId,
    string CanonicalCode,
    string DisplayCode,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] UnitOfMeasureDimension Dimension,
    bool IsEnabled,
    bool IsDefaultForDimension,
    string? DisplayNameOverride,
    int? SortOrder,
    string? Name);
