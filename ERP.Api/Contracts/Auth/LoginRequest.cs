namespace ERP.Api.Contracts.Auth;

public sealed record LoginRequest(string Email, string Password, DeviceInfoDto? DeviceInfo);
