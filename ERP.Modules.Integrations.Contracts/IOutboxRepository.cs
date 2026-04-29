using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Modules.Integrations.Contracts;

public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(
        int batchSize,
        DateTime utcNow,
        int maxAttempts,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default);
}
