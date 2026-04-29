namespace ERP.Modules.Identity.Application.Options;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";
    
    public int TokenExpiryMinutes { get; set; } = 15;
    public string FrontendResetUrl { get; set; } = string.Empty;
}