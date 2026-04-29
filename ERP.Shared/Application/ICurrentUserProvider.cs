namespace ERP.Shared.Application;

public interface ICurrentUserProvider
{
    long? UserId { get; }
    IReadOnlyCollection<string> Roles { get; }
    CurrentUser? GetCurrentUser();
    string? Source { get; }
}
