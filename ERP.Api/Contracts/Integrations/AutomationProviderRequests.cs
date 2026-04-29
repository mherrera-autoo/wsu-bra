using ERP.Modules.Integrations.Contracts;

namespace ERP.Api.Contracts.Integrations;

public sealed record AutomationProviderRegistrationRequest(
    string ProviderKey,
    string DisplayName,
    AutomationProviderType ProviderType,
    string? Endpoint,
    string? AuthenticationScheme);

public sealed record AutomationProviderRegistrationResponse(
    string ProviderKey,
    string DisplayName,
    AutomationProviderType ProviderType,
    string? Endpoint,
    string? AuthenticationScheme);

public sealed record AutomationDispatchRequest(string ProviderKey, string Operation, string Payload);
