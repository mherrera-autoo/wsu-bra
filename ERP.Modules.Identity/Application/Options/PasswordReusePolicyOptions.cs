namespace ERP.Modules.Identity.Application.Options;

public sealed class PasswordReusePolicyOptions
{
    public const string SectionName = "PasswordReusePolicy";
    
    public int DisallowLastN { get; set; } = 5;
}