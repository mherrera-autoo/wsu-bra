namespace ERP.Api.Contracts.Auth;

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Token, string NewPassword);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);