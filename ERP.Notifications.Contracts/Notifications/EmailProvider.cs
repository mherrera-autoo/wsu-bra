namespace ERP.Notifications.Contracts.Notifications;

public enum EmailProvider
{
    Smtp = 1,
    SendGrid = 2,
    AzureAcs = 3,
    AwsSes = 4
}