using ERP.Shared.Domain;

namespace ERP.Workflows.Domain;

public sealed class WorkflowInstance : Entity
{
    public long WorkflowDefinitionId { get; private set; }
    public string DocumentType { get; private set; } = null!;
    public string DocumentId { get; private set; } = null!;
    public long CurrentStateId { get; private set; }
    public WorkflowInstanceStatus Status { get; private set; }
    public string? ContextDataJson { get; private set; }
    public DateTime StartedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; private set; }

    private WorkflowInstance() { }

    public WorkflowInstance(long workflowDefinitionId, string documentType, string documentId, long initialStateId, string? contextDataJson)
    {
        if (string.IsNullOrWhiteSpace(documentType)) throw new ArgumentException("DocumentType is required.", nameof(documentType));
        if (string.IsNullOrWhiteSpace(documentId)) throw new ArgumentException("DocumentId is required.", nameof(documentId));
        if (initialStateId <= 0) throw new ArgumentOutOfRangeException(nameof(initialStateId));

        WorkflowDefinitionId = workflowDefinitionId;
        DocumentType = documentType.Trim();
        DocumentId = documentId.Trim();
        CurrentStateId = initialStateId;
        Status = WorkflowInstanceStatus.Active;
        ContextDataJson = string.IsNullOrWhiteSpace(contextDataJson) ? null : contextDataJson.Trim();
    }

    public void MoveTo(long stateId)
    {
        if (Status != WorkflowInstanceStatus.Active)
        {
            throw new InvalidOperationException("Cannot move a workflow that is not active.");
        }

        CurrentStateId = stateId;
    }

    public void Complete()
    {
        Status = WorkflowInstanceStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = WorkflowInstanceStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
}
