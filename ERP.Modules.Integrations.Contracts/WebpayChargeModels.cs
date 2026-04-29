namespace ERP.Modules.Integrations.Contracts;

public sealed record WebpayChargeRequest(
    long CompanyId,
    long SubscriptionId,
    decimal Amount,
    string Currency,
    string Description);

public sealed record WebpayChargeResult(
    bool Success,
    string? PaymentToken,
    string? Error);
