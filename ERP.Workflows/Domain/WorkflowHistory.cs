using ERP.Shared.Domain;

namespace ERP.Workflows.Domain;

public sealed class WorkflowHistory : Entity
{
    public long WorkflowInstanceId { get; private set; }
    public long? FromStateId { get; private set; }
    public long? ToStateId { get; private set; }
    public WorkflowActionType ActionType { get; private set; }
    public string? PerformedBy { get; private set; }
    public DateTime PerformedAtUtc { get; private set; } = DateTime.UtcNow;
    public string? Comment { get; private set; }

    private WorkflowHistory() { }

    public WorkflowHistory(
        long workflowInstanceId,
        long? fromStateId,
        long? toStateId,
        WorkflowActionType actionType,
        string? performedBy,
        string? comment)
    {
        if (workflowInstanceId <= 0) throw new ArgumentOutOfRangeException(nameof(workflowInstanceId));
        WorkflowInstanceId = workflowInstanceId;
        FromStateId = fromStateId;
        ToStateId = toStateId;
        ActionType = actionType;
        PerformedBy = string.IsNullOrWhiteSpace(performedBy) ? null : performedBy.Trim();
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
    }
}
