using ERP.Workflows.Domain;

namespace ERP.Workflows.Application.Repositories;

public interface IWorkflowDefinitionRepository
{
    Task AddAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default);
    Task<WorkflowDefinition?> GetActiveByDocumentTypeAsync(string documentType, CancellationToken cancellationToken = default);
    Task<WorkflowDefinition?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
