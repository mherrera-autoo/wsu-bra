using System.Threading;
using System.Threading.Tasks;

namespace ERP.Modules.Integrations.Contracts;

public interface IOutboxMessageHandler
{
    string MessageType { get; }
    Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default);
}
