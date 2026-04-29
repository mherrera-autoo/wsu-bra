using ERP.Workflows.Application.Repositories;
using ERP.Workflows.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class WorkflowHistoryRepository : IWorkflowHistoryRepository
{
    private readonly ErpDbContext _dbContext;

    public WorkflowHistoryRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(WorkflowHistory history, CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkflowHistories.AddAsync(history, cancellationToken);
    }

    public async Task<IReadOnlyList<WorkflowHistory>> GetByInstanceAsync(long workflowInstanceId, CancellationToken cancellationToken = default)
        => await _dbContext.WorkflowHistories
            .Where(history => history.WorkflowInstanceId == workflowInstanceId)
            .OrderBy(history => history.PerformedAtUtc)
            .ToListAsync(cancellationToken);
}
