namespace ERP.Modules.MasterData.Application.Services;

public sealed record CompanyFeatureState(
    Guid FeaturePublicId,
    bool IsActive,
    DateTime UpdatedAt);
