using System.Text.Json.Serialization;
using ERP.Modules.MasterData.Domain;

namespace ERP.Api.Contracts.MasterData;

public sealed record CreateUnitOfMeasureRequest(
    string CanonicalCode,
    string DisplayCode,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] UnitOfMeasureDimension Dimension,
    bool IsBaseUnit,
    decimal FactorToBase,
    int PrecisionScale,
    bool IsActive,
    int? SortOrder = null);
