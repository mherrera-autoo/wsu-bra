using ERP.Workflows.Application.Repositories;
using ERP.Workflows.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class WorkflowTaskRepository : IWorkflowTaskRepository
{
    private readonly ErpDbContext _dbContext;

    public WorkflowTaskRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(WorkflowTask task, CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkflowTasks.AddAsync(task, cancellationToken);
    }

    public Task<WorkflowTask?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.WorkflowTasks.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);

    public async Task<IReadOnlyList<WorkflowTask>> GetPendingByInstanceAsync(long workflowInstanceId, CancellationToken cancellationToken = default)
        => await _dbContext.WorkflowTasks
            .Where(task => task.WorkflowInstanceId == workflowInstanceId && task.Status == WorkflowTaskStatus.Pending)
            .ToListAsync(cancellationToken);

    public Task UpdateAsync(WorkflowTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkflowTasks.Update(task);
        return Task.CompletedTask;
    }
}
