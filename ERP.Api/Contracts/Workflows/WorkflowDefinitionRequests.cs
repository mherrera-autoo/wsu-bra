namespace ERP.Api.Contracts.Workflows;

public sealed record WorkflowDefinitionCreateRequest(
    string Name,
    string DocumentType,
    IReadOnlyList<WorkflowStateCreateRequest> States,
    IReadOnlyList<WorkflowTransitionCreateRequest> Transitions);

public sealed record WorkflowStateCreateRequest(
    string Name,
    bool IsInitial,
    bool IsFinal,
    bool RequiresApproval,
    string? AssignedRole,
    long? AssignedUserId);

public sealed record WorkflowTransitionCreateRequest(
    string FromState,
    string ToState,
    string ActionType,
    string? RuleJson);
