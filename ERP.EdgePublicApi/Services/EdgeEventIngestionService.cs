using System.Text.Json;
using ERP.EdgePublicApi.Contracts.Edge;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Rfid.Domain;
using ERP.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ERP.EdgePublicApi.Services;

public sealed class EdgeEventIngestionService
{
    private readonly ErpDbContext _dbContext;
    private readonly ILogger<EdgeEventIngestionService> _logger;

    public EdgeEventIngestionService(ErpDbContext dbContext, ILogger<EdgeEventIngestionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<EdgeBatchResultItem> IngestAsync(
        Guid companyPublicId,
        string edgeNodeId,
        EdgeBusinessEventRequest request,
        CancellationToken cancellationToken)
    {
        var eventId = request.EventId.Trim();
        var existing = await _dbContext.EdgeBusinessEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.EventId == eventId, cancellationToken);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Edge event idempotent replay edgeNodeId={EdgeNodeId} eventId={EventId} eventType={EventType}",
                edgeNodeId,
                eventId,
                request.EventType);

            return new EdgeBatchResultItem(eventId, "Accepted", "Idempotent replay.");
        }

        var normalizedEpcHex = request.EpcHex.Trim().ToUpperInvariant();
        var metadataJson = JsonSerializer.Serialize(request.Metadata ?? new Dictionary<string, object?>());
        var eventEntity = EdgeBusinessEvent.Create(
            companyPublicId,
            edgeNodeId,
            eventId,
            request.EventType.Trim(),
            request.SchemaVersion,
            request.OccurredAtUtc,
            request.OrderId,
            normalizedEpcHex,
            request.MovementType,
            request.FromZoneId,
            request.ToZoneId,
            request.ZoneId,
            request.ReaderId,
            request.AntennaId,
            request.Rssi,
            request.Reason,
            metadataJson);

        await _dbContext.EdgeBusinessEvents.AddAsync(eventEntity, cancellationToken);

        var outboxPayload = JsonSerializer.Serialize(new
        {
            eventEntity.EventId,
            eventEntity.EventType,
            eventEntity.EdgeNodeId,
            eventEntity.CompanyPublicId,
            eventEntity.OccurredAtUtc,
            eventEntity.OrderId,
            eventEntity.EpcHex,
            eventEntity.ReaderId
        });

        await _dbContext.OutboxMessages.AddAsync(
            OutboxMessage.Create("edge.business.event.received", outboxPayload),
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Edge event accepted edgeNodeId={EdgeNodeId} eventId={EventId} eventType={EventType}",
            edgeNodeId,
            eventId,
            request.EventType);

        return new EdgeBatchResultItem(eventId, "Accepted", null);
    }
}
