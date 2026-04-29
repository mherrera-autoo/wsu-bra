using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.PharmaceuticalRegulatedInventory.Contracts;

namespace ERP.Modules.Sales.Application.Handlers;

// TODO: Move dispensing outbox handler to ERP.Modules.RetailPharmacy.
public sealed class PharmacyDispenseConfirmedHandler : IOutboxMessageHandler
{
    public string MessageType => "pharmacy.dispense.confirmed";

    public Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        var dispense = JsonSerializer.Deserialize<PharmacyDispenseConfirmed>(payloadJson);
        if (dispense is null)
        {
            throw new InvalidOperationException("Invalid pharmacy dispense payload.");
        }

        return Task.CompletedTask;
    }
}
