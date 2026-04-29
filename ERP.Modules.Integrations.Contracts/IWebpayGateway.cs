namespace ERP.Modules.Integrations.Contracts;

public interface IWebpayGateway
{
    Task<WebpayChargeResult> CreateChargeAsync(WebpayChargeRequest request, CancellationToken cancellationToken = default);
}
