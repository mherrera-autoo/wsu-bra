namespace ERP.Shared.Application;

public interface IEventHandler<in TEvent>
{
    Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}
