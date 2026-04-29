namespace ERP.Modules.Identity.Application.Services;

public sealed class WorkspaceResolutionException : Exception
{
    public WorkspaceResolutionException(string message)
        : base(message)
    {
    }
}
