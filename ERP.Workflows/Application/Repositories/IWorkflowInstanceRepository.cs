using ERP.Workflows.Domain;

namespace ERP.Workflows.Application.Repositories;

public interface IWorkflowInstanceRepository
{
    Task AddAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);
    Task<WorkflowInstance?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<WorkflowInstance?> GetByDocumentAsync(string documentType, string documentId, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default);
}
