using ERP.Shared.Domain;

namespace ERP.Workflows.Domain;

public sealed class WorkflowTask : Entity
{
    public long WorkflowInstanceId { get; private set; }
    public long WorkflowStateId { get; private set; }
    public WorkflowTaskStatus Status { get; private set; }
    public string? AssignedRole { get; private set; }
    public long? AssignedUserId { get; private set; }
    public WorkflowActionType? CompletedAction { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; private set; }

    private WorkflowTask() { }

    public WorkflowTask(long workflowInstanceId, long workflowStateId, string? assignedRole, long? assignedUserId)
    {
        if (workflowInstanceId <= 0) throw new ArgumentOutOfRangeException(nameof(workflowInstanceId));
        if (workflowStateId <= 0) throw new ArgumentOutOfRangeException(nameof(workflowStateId));
        WorkflowInstanceId = workflowInstanceId;
        WorkflowStateId = workflowStateId;
        AssignedRole = string.IsNullOrWhiteSpace(assignedRole) ? null : assignedRole.Trim();
        AssignedUserId = assignedUserId;
        Status = WorkflowTaskStatus.Pending;
    }

    public void Complete(WorkflowActionType actionType)
    {
        if (Status != WorkflowTaskStatus.Pending)
        {
            throw new InvalidOperationException("Task already completed.");
        }

        Status = WorkflowTaskStatus.Completed;
        CompletedAction = actionType;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status != WorkflowTaskStatus.Pending)
        {
            return;
        }

        Status = WorkflowTaskStatus.Cancelled;
        CompletedAtUtc = DateTime.UtcNow;
    }
}
