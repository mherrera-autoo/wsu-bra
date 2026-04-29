using ERP.Modules.Integrations.Contracts;

namespace ERP.Modules.Integrations.Infrastructure;

public sealed class WebpayGateway : IWebpayGateway
{
    public Task<WebpayChargeResult> CreateChargeAsync(WebpayChargeRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount < 0)
        {
            return Task.FromResult(new WebpayChargeResult(false, null, "Amount must be non-negative."));
        }

        var token = $"webpay_{Guid.NewGuid():N}";
        return Task.FromResult(new WebpayChargeResult(true, token, null));
    }
}
