namespace ERP.Api.Contracts.Auth;

public sealed record SessionResponse(long Id, string DeviceInfo, DateTime LastSeenAt, DateTime ExpiresAt, DateTime CreatedAt);
