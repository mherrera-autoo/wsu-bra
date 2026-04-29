using ERP.Modules.Integrations.Contracts;
using System;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly ErpDbContext _dbContext;

    public OutboxRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        await _dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(
        int batchSize,
        DateTime utcNow,
        int maxAttempts,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .Where(m => !m.IsDeadLettered)
            .Where(m => m.Attempts < maxAttempts)
            .Where(m => m.NextRetryAt == null || m.NextRetryAt <= utcNow)
            .OrderBy(m => m.OccurredAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        _dbContext.OutboxMessages.Update(message);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
