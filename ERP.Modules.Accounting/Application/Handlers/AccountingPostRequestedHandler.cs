using ERP.Modules.Accounting.Application.Services;
using ERP.Shared.Application;
using ERP.Modules.Accounting.Contracts;

namespace ERP.Modules.Accounting.Application.Handlers;

public sealed class AccountingPostRequestedHandler : IEventHandler<AccountingPostRequested>
{
    private readonly AccountingPostingService _postingService;

    public AccountingPostRequestedHandler(AccountingPostingService postingService)
    {
        _postingService = postingService;
    }

    public async Task HandleAsync(AccountingPostRequested @event, CancellationToken cancellationToken = default)
    {
        var result = await _postingService.PostAsync(@event, cancellationToken);
        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to post journal entry.");
        }
    }
}
