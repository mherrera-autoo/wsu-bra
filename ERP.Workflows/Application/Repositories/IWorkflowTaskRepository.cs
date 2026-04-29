using ERP.Workflows.Domain;

namespace ERP.Workflows.Application.Repositories;

public interface IWorkflowTaskRepository
{
    Task AddAsync(WorkflowTask task, CancellationToken cancellationToken = default);
    Task<WorkflowTask?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowTask>> GetPendingByInstanceAsync(long workflowInstanceId, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkflowTask task, CancellationToken cancellationToken = default);
}
