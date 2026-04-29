namespace ERP.Notifications.Contracts.Notifications;

public sealed class EmailOptions
{
    public EmailProvider Provider { get; set; } = EmailProvider.Smtp;
    public string FromEmail { get; set; } = "";
    public string FromName { get; set; } = "";
    public SmtpOptions Smtp { get; set; } = new();
    public SendGridOptions SendGrid { get; set; } = new();
    public AzureAcsOptions AzureAcs { get; set; } = new();
    public AwsSesOptions AwsSes { get; set; } = new();
}

public sealed class SmtpOptions
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool UseTls { get; set; } = true;
    public SmtpAuthMode AuthMode { get; set; } = SmtpAuthMode.Basic;

    // Basic
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";

    // OAuth2
    public SmtpOAuth2Options OAuth2 { get; set; } = new();
}

public sealed class SmtpOAuth2Options
{
    public string Provider { get; set; } = "Google";
    public string UserEmail { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string RefreshToken { get; set; } = "";
}

public sealed class SendGridOptions
{
    public string ApiKey { get; set; } = "";
}

public sealed class AzureAcsOptions
{
    public string ConnectionString { get; set; } = "";
}

public sealed class AwsSesOptions
{
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string Region { get; set; } = "us-east-1";
}