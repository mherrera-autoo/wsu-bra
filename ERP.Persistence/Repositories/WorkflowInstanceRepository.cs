using ERP.Workflows.Application.Repositories;
using ERP.Workflows.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class WorkflowInstanceRepository : IWorkflowInstanceRepository
{
    private readonly ErpDbContext _dbContext;

    public WorkflowInstanceRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkflowInstances.AddAsync(instance, cancellationToken);
    }

    public Task<WorkflowInstance?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.WorkflowInstances.FirstOrDefaultAsync(instance => instance.Id == id, cancellationToken);

    public Task<WorkflowInstance?> GetByDocumentAsync(string documentType, string documentId, CancellationToken cancellationToken = default)
        => _dbContext.WorkflowInstances
            .FirstOrDefaultAsync(instance => instance.DocumentType == documentType && instance.DocumentId == documentId, cancellationToken);

    public Task UpdateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
    {
        _dbContext.WorkflowInstances.Update(instance);
        return Task.CompletedTask;
    }
}
