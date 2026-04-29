namespace ERP.Api.Contracts.Identity;

public sealed record BootstrapResponse(bool Completed, bool CanBootstrap, long? AdminUserId, string Message);
