using ERP.Modules.Integrations.Contracts;

namespace ERP.Modules.Integrations.Infrastructure;

public sealed class NoOpAutomationProvider : IAutomationProvider
{
    public string ProviderKey => "noop";
    public AutomationProviderType ProviderType => AutomationProviderType.Custom;

    public Task<AutomationResponse> ExecuteAsync(AutomationRequest request, CancellationToken cancellationToken = default)
    {
        var response = new AutomationResponse(true, "No-op provider acknowledged request.", Guid.NewGuid().ToString("N"));
        return Task.FromResult(response);
    }
}
