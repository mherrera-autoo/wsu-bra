using ERP.Shared.Domain;

namespace ERP.Workflows.Domain;

public sealed class WorkflowDefinition : Entity
{
    private readonly List<WorkflowState> _states = new();
    private readonly List<WorkflowTransition> _transitions = new();

    public string Name { get; private set; } = null!;
    public string DocumentType { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    public IReadOnlyCollection<WorkflowState> States => _states;
    public IReadOnlyCollection<WorkflowTransition> Transitions => _transitions;

    private WorkflowDefinition() { }

    public WorkflowDefinition(string name, string documentType)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(documentType)) throw new ArgumentException("DocumentType is required.", nameof(documentType));
        Name = name.Trim();
        DocumentType = documentType.Trim();
    }

    public WorkflowState AddState(string name, bool isInitial, bool isFinal, bool requiresApproval, string? assignedRole, long? assignedUserId)
    {
        if (_states.Any(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"State '{name}' already exists.");
        }

        if (isInitial && _states.Any(s => s.IsInitial))
        {
            throw new InvalidOperationException("Workflow already has an initial state.");
        }

        var state = new WorkflowState(Id, name, isInitial, isFinal, requiresApproval, assignedRole, assignedUserId);
        _states.Add(state);
        return state;
    }

    public WorkflowTransition AddTransition(WorkflowState fromState, WorkflowState toState, WorkflowActionType actionType, string? ruleJson)
    {
        if (fromState is null) throw new ArgumentNullException(nameof(fromState));
        if (toState is null) throw new ArgumentNullException(nameof(toState));

        var transition = new WorkflowTransition(this, fromState, toState, actionType, ruleJson);
        _transitions.Add(transition);
        return transition;
    }

    public WorkflowState GetInitialState()
    {
        var state = _states.FirstOrDefault(s => s.IsInitial);
        if (state is null)
        {
            throw new InvalidOperationException("Workflow has no initial state.");
        }

        return state;
    }

    public void Deactivate() => IsActive = false;
}
