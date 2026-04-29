using ERP.Workflows.Domain;

namespace ERP.Workflows.Application.Repositories;

public interface IWorkflowHistoryRepository
{
    Task AddAsync(WorkflowHistory history, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<WorkflowHistory>> GetByInstanceAsync(long workflowInstanceId, CancellationToken cancellationToken = default);
}
