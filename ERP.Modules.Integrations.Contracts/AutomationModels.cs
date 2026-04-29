namespace ERP.Modules.Integrations.Contracts;

public enum AutomationProviderType
{
    Banking = 1,
    Tax = 2,
    Custom = 3
}

public sealed record AutomationProviderRegistration(
    long CompanyId,
    string ProviderKey,
    string DisplayName,
    AutomationProviderType ProviderType,
    string? Endpoint,
    string? AuthenticationScheme);

public sealed record AutomationRequest(
    long CompanyId,
    string ProviderKey,
    string Operation,
    string Payload);

public sealed record AutomationResponse(
    bool Success,
    string? Message,
    string? ExternalReference);

public interface IAutomationProvider
{
    string ProviderKey { get; }
    AutomationProviderType ProviderType { get; }
    Task<AutomationResponse> ExecuteAsync(AutomationRequest request, CancellationToken cancellationToken = default);
}

public interface IAutomationProviderRegistry
{
    Task RegisterAsync(AutomationProviderRegistration registration, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AutomationProviderRegistration>> ListAsync(long companyId, CancellationToken cancellationToken = default);
    Task<AutomationResponse> DispatchAsync(
        long companyId,
        string providerKey,
        AutomationRequest request,
        CancellationToken cancellationToken = default);
}
