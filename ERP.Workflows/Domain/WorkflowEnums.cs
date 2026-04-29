namespace ERP.Workflows.Domain;

public enum WorkflowInstanceStatus
{
    Active = 1,
    Completed = 2,
    Cancelled = 3
}

public enum WorkflowTaskStatus
{
    Pending = 1,
    Completed = 2,
    Cancelled = 3
}

public enum WorkflowActionType
{
    Auto = 1,
    Approve = 2,
    Reject = 3
}
