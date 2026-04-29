using ERP.Modules.Integrations.Contracts;

namespace ERP.Modules.Integrations.Infrastructure;

public sealed class InMemoryAutomationProviderRegistry : IAutomationProviderRegistry
{
    private readonly Dictionary<long, List<AutomationProviderRegistration>> _registrations = new();
    private readonly Dictionary<string, IAutomationProvider> _providers;

    public InMemoryAutomationProviderRegistry(IEnumerable<IAutomationProvider> providers)
    {
        _providers = providers.ToDictionary(provider => provider.ProviderKey, StringComparer.OrdinalIgnoreCase);
    }

    public Task RegisterAsync(AutomationProviderRegistration registration, CancellationToken cancellationToken = default)
    {
        if (!_registrations.TryGetValue(registration.CompanyId, out var list))
        {
            list = new List<AutomationProviderRegistration>();
            _registrations[registration.CompanyId] = list;
        }

        var existingIndex = list.FindIndex(item =>
            item.ProviderKey.Equals(registration.ProviderKey, StringComparison.OrdinalIgnoreCase));
        if (existingIndex >= 0)
        {
            list[existingIndex] = registration;
        }
        else
        {
            list.Add(registration);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AutomationProviderRegistration>> ListAsync(long companyId, CancellationToken cancellationToken = default)
    {
        _registrations.TryGetValue(companyId, out var list);
        return Task.FromResult<IReadOnlyList<AutomationProviderRegistration>>(list ?? new List<AutomationProviderRegistration>());
    }

    public Task<AutomationResponse> DispatchAsync(
        long companyId,
        string providerKey,
        AutomationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_providers.TryGetValue(providerKey, out var provider))
        {
            return Task.FromResult(new AutomationResponse(false, "Provider not registered.", null));
        }

        if (request.CompanyId != companyId)
        {
            return Task.FromResult(new AutomationResponse(false, "Company mismatch for provider dispatch.", null));
        }

        return provider.ExecuteAsync(request, cancellationToken);
    }
}
