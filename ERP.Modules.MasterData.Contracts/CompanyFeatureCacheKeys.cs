namespace ERP.Modules.MasterData.Contracts;

public static class CompanyFeatureCacheKeys
{
    public static string CompanyFeatures(long companyId) => $"company:{companyId}:features";
    public static string CompanyFeatures(Guid companyPublicId) => $"company:{companyPublicId}:features";
}
