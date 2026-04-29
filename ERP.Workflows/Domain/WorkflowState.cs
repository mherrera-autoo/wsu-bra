using ERP.Shared.Domain;

namespace ERP.Workflows.Domain;

public sealed class WorkflowState : Entity
{
    public long WorkflowDefinitionId { get; private set; }
    public string Name { get; private set; } = null!;
    public bool IsInitial { get; private set; }
    public bool IsFinal { get; private set; }
    public bool RequiresApproval { get; private set; }
    public string? AssignedRole { get; private set; }
    public long? AssignedUserId { get; private set; }

    private WorkflowState() { }

    public WorkflowState(
        long workflowDefinitionId,
        string name,
        bool isInitial,
        bool isFinal,
        bool requiresApproval,
        string? assignedRole,
        long? assignedUserId)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        WorkflowDefinitionId = workflowDefinitionId;
        Name = name.Trim();
        IsInitial = isInitial;
        IsFinal = isFinal;
        RequiresApproval = requiresApproval;
        AssignedRole = string.IsNullOrWhiteSpace(assignedRole) ? null : assignedRole.Trim();
        AssignedUserId = assignedUserId;
    }
}
