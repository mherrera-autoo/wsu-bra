using System.Text.Json.Serialization;
using ERP.Modules.MasterData.Domain;

namespace ERP.Api.Contracts.MasterData;

public sealed record UnitOfMeasureSummary(
    long Id,
    string CanonicalCode,
    string DisplayCode,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] UnitOfMeasureDimension Dimension,
    bool IsBaseUnit,
    decimal FactorToBase,
    int PrecisionScale,
    bool IsActive,
    string? Name,
    string? Symbol);
