using ERP.Shared.Domain;

namespace ERP.Workflows.Domain;

public sealed class WorkflowTransition : Entity
{
    public long WorkflowDefinitionId { get; private set; }
    public long FromStateId { get; private set; }
    public long ToStateId { get; private set; }
    public WorkflowActionType ActionType { get; private set; }
    public string? RuleJson { get; private set; }
    public WorkflowState? FromState { get; private set; }
    public WorkflowState? ToState { get; private set; }

    private WorkflowTransition() { }

    public WorkflowTransition(WorkflowDefinition definition, WorkflowState fromState, WorkflowState toState, WorkflowActionType actionType, string? ruleJson)
    {
        if (definition is null) throw new ArgumentNullException(nameof(definition));
        if (fromState is null) throw new ArgumentNullException(nameof(fromState));
        if (toState is null) throw new ArgumentNullException(nameof(toState));
        WorkflowDefinitionId = definition.Id;
        FromState = fromState;
        ToState = toState;
        FromStateId = fromState.Id;
        ToStateId = toState.Id;
        ActionType = actionType;
        RuleJson = string.IsNullOrWhiteSpace(ruleJson) ? null : ruleJson.Trim();
    }
}
