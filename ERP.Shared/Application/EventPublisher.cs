using Microsoft.Extensions.DependencyInjection;

namespace ERP.Shared.Application;

public sealed class EventPublisher : IEventPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public EventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event is null) throw new ArgumentNullException(nameof(@event));

        var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(@event, cancellationToken);
        }
    }
}
