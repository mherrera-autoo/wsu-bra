namespace ERP.Modules.Identity.Application.Services;

public enum RefreshTokenStatus
{
    Success = 0,
    ReusedWithinGrace = 1,
    Invalid = 2,
    Compromised = 3,
    RateLimited = 4
}

public sealed record RefreshTokenResult(
    RefreshTokenStatus Status,
    AuthResult? Auth = null,
    string? Error = null,
    TimeSpan? RetryAfter = null)
{
    public bool IsSuccess => Status is RefreshTokenStatus.Success or RefreshTokenStatus.ReusedWithinGrace;
}
