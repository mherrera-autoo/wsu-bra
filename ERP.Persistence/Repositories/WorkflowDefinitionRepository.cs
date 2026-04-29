using ERP.Workflows.Application.Repositories;
using ERP.Workflows.Domain;
using Microsoft.EntityFrameworkCore;

namespace ERP.Persistence.Repositories;

public sealed class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly ErpDbContext _dbContext;

    public WorkflowDefinitionRepository(ErpDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        await _dbContext.WorkflowDefinitions.AddAsync(definition, cancellationToken);
    }

    public Task<WorkflowDefinition?> GetActiveByDocumentTypeAsync(string documentType, CancellationToken cancellationToken = default)
        => _dbContext.WorkflowDefinitions
            .Include(definition => definition.States)
            .Include(definition => definition.Transitions)
            .FirstOrDefaultAsync(definition => definition.IsActive && definition.DocumentType == documentType, cancellationToken);

    public Task<WorkflowDefinition?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
        => _dbContext.WorkflowDefinitions
            .Include(definition => definition.States)
            .Include(definition => definition.Transitions)
            .FirstOrDefaultAsync(definition => definition.Id == id, cancellationToken);
}
