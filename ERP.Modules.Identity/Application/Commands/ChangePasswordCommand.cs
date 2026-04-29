namespace ERP.Modules.Identity.Application.Commands;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword);