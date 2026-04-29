namespace ERP.Workflows.Contracts;

public sealed record WorkflowInstanceStarted(
    long InstanceId,
    long DefinitionId,
    string DocumentType,
    string DocumentId,
    long CurrentStateId,
    DateTime StartedAt,
    string? ContextDataJson);

public sealed record WorkflowInstanceCompleted(
    long InstanceId,
    WorkflowInstanceStatus Status,
    DateTime CompletedAt);

public sealed record WorkflowTaskCreated(
    long TaskId,
    long InstanceId,
    long StateId,
    string? AssignedRole,
    long? AssignedUserId,
    DateTime CreatedAtUtc);

public sealed record WorkflowTaskCompleted(
    long TaskId,
    long InstanceId,
    long StateId,
    WorkflowActionType ActionType,
    DateTime CompletedAtUtc);

public enum WorkflowInstanceStatus
{
    Active = 1,
    Completed = 2,
    Cancelled = 3
}

public enum WorkflowActionType
{
    Auto = 1,
    Approve = 2,
    Reject = 3
}
