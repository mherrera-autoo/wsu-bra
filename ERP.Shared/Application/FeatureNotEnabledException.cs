namespace ERP.Shared.Application;

public sealed class FeatureNotEnabledException : Exception
{
    public FeatureNotEnabledException(string featureKey)
        : base($"Feature '{featureKey}' is not enabled.")
    {
        FeatureKey = featureKey;
    }

    public string FeatureKey { get; }
}
