namespace ERP.Shared.Application;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default);
}
