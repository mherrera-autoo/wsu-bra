namespace ERP.Modules.Identity.Application.Commands;

public sealed record ResetPasswordCommand(string Token, string NewPassword);