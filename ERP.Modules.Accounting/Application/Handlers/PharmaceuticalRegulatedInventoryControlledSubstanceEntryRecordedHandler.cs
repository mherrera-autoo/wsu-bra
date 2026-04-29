using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.PharmaceuticalRegulatedInventory.Contracts;

namespace ERP.Modules.Accounting.Application.Handlers;

public sealed class PharmaceuticalRegulatedInventoryControlledSubstanceEntryRecordedHandler : IOutboxMessageHandler
{
    public string MessageType => "pharmacy.controlled-substance.entry.recorded";

    public Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        var entry = JsonSerializer.Deserialize<PharmaceuticalRegulatedInventoryControlledSubstanceEntryRecorded>(payloadJson);
        if (entry is null)
        {
            throw new InvalidOperationException("Invalid controlled substance payload.");
        }

        return Task.CompletedTask;
    }
}
