using System.Text.Json;
using ERP.Documents.Contracts;
using ERP.Modules.Integrations.Contracts;
using ERP.Workflows.Application.Repositories;
using WorkflowContracts = ERP.Workflows.Contracts;
using ERP.Workflows.Domain;
using ERP.Shared.Application;

namespace ERP.Workflows.Application.Services;

public sealed class WorkflowService
{
    private readonly IWorkflowDefinitionRepository _definitionRepository;
    private readonly IWorkflowInstanceRepository _instanceRepository;
    private readonly IWorkflowTaskRepository _taskRepository;
    private readonly IWorkflowHistoryRepository _historyRepository;
    private readonly IOutboxRepository _outboxRepository;
    private readonly WorkflowRuleEvaluator _ruleEvaluator;
    private readonly IUnitOfWork _unitOfWork;

    public WorkflowService(
        IWorkflowDefinitionRepository definitionRepository,
        IWorkflowInstanceRepository instanceRepository,
        IWorkflowTaskRepository taskRepository,
        IWorkflowHistoryRepository historyRepository,
        IOutboxRepository outboxRepository,
        WorkflowRuleEvaluator ruleEvaluator,
        IUnitOfWork unitOfWork)
    {
        _definitionRepository = definitionRepository;
        _instanceRepository = instanceRepository;
        _taskRepository = taskRepository;
        _historyRepository = historyRepository;
        _outboxRepository = outboxRepository;
        _ruleEvaluator = ruleEvaluator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<WorkflowDefinition>> CreateDefinitionAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            await _definitionRepository.AddAsync(definition, token);
            return Result<WorkflowDefinition>.Ok(definition);
        }, cancellationToken);
    }

    public async Task<Result<WorkflowInstance>> StartWorkflowAsync(
        string documentType,
        string documentId,
        string? contextDataJson,
        string? performedBy,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var existing = await _instanceRepository.GetByDocumentAsync(documentType, documentId, token);
            if (existing is not null)
            {
                return Result<WorkflowInstance>.Fail("Workflow already exists for the document.");
            }

            var definition = await _definitionRepository.GetActiveByDocumentTypeAsync(documentType, token);
            if (definition is null)
            {
                return Result<WorkflowInstance>.Fail("No active workflow definition for the document type.");
            }

            var initialState = definition.GetInitialState();
            var instance = new WorkflowInstance(definition.Id, definition.DocumentType, documentId, initialState.Id, contextDataJson);
            await _instanceRepository.AddAsync(instance, token);
            await _historyRepository.AddAsync(new WorkflowHistory(instance.Id, null, initialState.Id, WorkflowActionType.Auto, performedBy, "Workflow started."), token);
            await PublishDocumentLinkedAsync(instance, token);
            await PublishInstanceStartedAsync(instance, token);

            await ApplyStateSideEffectsAsync(definition, instance, initialState, performedBy, token);
            return Result<WorkflowInstance>.Ok(instance);
        }, cancellationToken);
    }

    public async Task<Result<WorkflowInstance>> ApproveTaskAsync(long taskId, string? performedBy, string? comment, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var task = await _taskRepository.GetByIdAsync(taskId, token);
            if (task is null)
            {
                return Result<WorkflowInstance>.Fail("Task not found.");
            }

            if (task.Status != WorkflowTaskStatus.Pending)
            {
                return Result<WorkflowInstance>.Fail("Task is not pending.");
            }

            var instance = await _instanceRepository.GetByIdAsync(task.WorkflowInstanceId, token);
            if (instance is null)
            {
                return Result<WorkflowInstance>.Fail("Workflow instance not found.");
            }

            var definition = await _definitionRepository.GetByIdAsync(instance.WorkflowDefinitionId, token);
            if (definition is null)
            {
                return Result<WorkflowInstance>.Fail("Workflow definition not found.");
            }

            var currentState = definition.States.First(state => state.Id == instance.CurrentStateId);
            var transition = FindTransition(definition, currentState.Id, WorkflowActionType.Approve, instance.ContextDataJson);
            if (transition is null)
            {
                return Result<WorkflowInstance>.Fail("No valid approve transition found.");
            }

            task.Complete(WorkflowActionType.Approve);
            await _taskRepository.UpdateAsync(task, token);
            await PublishTaskCompletedAsync(task, token);

            instance.MoveTo(transition.ToStateId);
            await _instanceRepository.UpdateAsync(instance, token);
            await _historyRepository.AddAsync(new WorkflowHistory(instance.Id, currentState.Id, transition.ToStateId, WorkflowActionType.Approve, performedBy, comment), token);

            var nextState = definition.States.First(state => state.Id == transition.ToStateId);
            await ApplyStateSideEffectsAsync(definition, instance, nextState, performedBy, token);
            return Result<WorkflowInstance>.Ok(instance);
        }, cancellationToken);
    }

    public async Task<Result<WorkflowInstance>> RejectTaskAsync(long taskId, string? performedBy, string? comment, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var task = await _taskRepository.GetByIdAsync(taskId, token);
            if (task is null)
            {
                return Result<WorkflowInstance>.Fail("Task not found.");
            }

            if (task.Status != WorkflowTaskStatus.Pending)
            {
                return Result<WorkflowInstance>.Fail("Task is not pending.");
            }

            var instance = await _instanceRepository.GetByIdAsync(task.WorkflowInstanceId, token);
            if (instance is null)
            {
                return Result<WorkflowInstance>.Fail("Workflow instance not found.");
            }

            var definition = await _definitionRepository.GetByIdAsync(instance.WorkflowDefinitionId, token);
            if (definition is null)
            {
                return Result<WorkflowInstance>.Fail("Workflow definition not found.");
            }

            var currentState = definition.States.First(state => state.Id == instance.CurrentStateId);
            var transition = FindTransition(definition, currentState.Id, WorkflowActionType.Reject, instance.ContextDataJson);
            if (transition is null)
            {
                return Result<WorkflowInstance>.Fail("No valid reject transition found.");
            }

            task.Complete(WorkflowActionType.Reject);
            await _taskRepository.UpdateAsync(task, token);
            await PublishTaskCompletedAsync(task, token);

            instance.MoveTo(transition.ToStateId);
            await _instanceRepository.UpdateAsync(instance, token);
            await _historyRepository.AddAsync(new WorkflowHistory(instance.Id, currentState.Id, transition.ToStateId, WorkflowActionType.Reject, performedBy, comment), token);

            var nextState = definition.States.First(state => state.Id == transition.ToStateId);
            await ApplyStateSideEffectsAsync(definition, instance, nextState, performedBy, token);
            return Result<WorkflowInstance>.Ok(instance);
        }, cancellationToken);
    }

    public Task<IReadOnlyList<WorkflowHistory>> GetHistoryAsync(long workflowInstanceId, CancellationToken cancellationToken = default)
        => _historyRepository.GetByInstanceAsync(workflowInstanceId, cancellationToken);

    private async Task ApplyStateSideEffectsAsync(
        WorkflowDefinition definition,
        WorkflowInstance instance,
        WorkflowState state,
        string? performedBy,
        CancellationToken cancellationToken)
    {
        if (state.IsFinal)
        {
            instance.Complete();
            await _instanceRepository.UpdateAsync(instance, cancellationToken);
            await PublishDocumentArchivedAsync(instance, cancellationToken);
            await PublishInstanceCompletedAsync(instance, cancellationToken);
            return;
        }

        if (state.RequiresApproval)
        {
            var pendingTasks = await _taskRepository.GetPendingByInstanceAsync(instance.Id, cancellationToken);
            foreach (var pendingTask in pendingTasks)
            {
                pendingTask.Cancel();
                await _taskRepository.UpdateAsync(pendingTask, cancellationToken);
            }

            var task = new WorkflowTask(instance.Id, state.Id, state.AssignedRole, state.AssignedUserId);
            await _taskRepository.AddAsync(task, cancellationToken);
            await _historyRepository.AddAsync(new WorkflowHistory(instance.Id, state.Id, state.Id, WorkflowActionType.Auto, performedBy, "Approval task created."), cancellationToken);
            await PublishTaskCreatedAsync(task, cancellationToken);
            return;
        }

        await ApplyAutoTransitionsAsync(definition, instance, state, performedBy, cancellationToken);
    }

    private async Task ApplyAutoTransitionsAsync(
        WorkflowDefinition definition,
        WorkflowInstance instance,
        WorkflowState currentState,
        string? performedBy,
        CancellationToken cancellationToken)
    {
        var state = currentState;
        while (true)
        {
            var transition = FindTransition(definition, state.Id, WorkflowActionType.Auto, instance.ContextDataJson);
            if (transition is null)
            {
                break;
            }

            instance.MoveTo(transition.ToStateId);
            await _instanceRepository.UpdateAsync(instance, cancellationToken);
            await _historyRepository.AddAsync(new WorkflowHistory(instance.Id, state.Id, transition.ToStateId, WorkflowActionType.Auto, performedBy, "Auto transition executed."), cancellationToken);

            state = definition.States.First(s => s.Id == transition.ToStateId);
            if (state.IsFinal)
            {
                instance.Complete();
                await _instanceRepository.UpdateAsync(instance, cancellationToken);
                await PublishInstanceCompletedAsync(instance, cancellationToken);
                break;
            }

            if (state.RequiresApproval)
            {
                var task = new WorkflowTask(instance.Id, state.Id, state.AssignedRole, state.AssignedUserId);
                await _taskRepository.AddAsync(task, cancellationToken);
                await _historyRepository.AddAsync(new WorkflowHistory(instance.Id, state.Id, state.Id, WorkflowActionType.Auto, performedBy, "Approval task created."), cancellationToken);
                await PublishTaskCreatedAsync(task, cancellationToken);
                break;
            }
        }
    }

    private async Task PublishInstanceStartedAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new WorkflowContracts.WorkflowInstanceStarted(
            instance.Id,
            instance.WorkflowDefinitionId,
            instance.DocumentType,
            instance.DocumentId,
            instance.CurrentStateId,
            instance.StartedAt,
            instance.ContextDataJson));
        await _outboxRepository.AddAsync(OutboxMessage.Create("workflow.instance.started", payload), cancellationToken);
    }

    private async Task PublishInstanceCompletedAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        if (instance.CompletedAt is null)
        {
            return;
        }

        var status = instance.Status == Domain.WorkflowInstanceStatus.Cancelled
            ? WorkflowContracts.WorkflowInstanceStatus.Cancelled
            : WorkflowContracts.WorkflowInstanceStatus.Completed;
        var payload = JsonSerializer.Serialize(new WorkflowContracts.WorkflowInstanceCompleted(
            instance.Id,
            status,
            instance.CompletedAt.Value));
        await _outboxRepository.AddAsync(OutboxMessage.Create("workflow.instance.completed", payload), cancellationToken);
    }

    private async Task PublishTaskCreatedAsync(WorkflowTask task, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(new WorkflowContracts.WorkflowTaskCreated(
            task.Id,
            task.WorkflowInstanceId,
            task.WorkflowStateId,
            task.AssignedRole,
            task.AssignedUserId,
            task.CreatedAtUtc));
        await _outboxRepository.AddAsync(OutboxMessage.Create("workflow.task.created", payload), cancellationToken);
    }

    private async Task PublishTaskCompletedAsync(WorkflowTask task, CancellationToken cancellationToken)
    {
        if (task.CompletedAtUtc is null || task.CompletedAction is null)
        {
            return;
        }

        var action = task.CompletedAction == Domain.WorkflowActionType.Approve
            ? WorkflowContracts.WorkflowActionType.Approve
            : task.CompletedAction == Domain.WorkflowActionType.Reject
                ? WorkflowContracts.WorkflowActionType.Reject
                : WorkflowContracts.WorkflowActionType.Auto;
        var payload = JsonSerializer.Serialize(new WorkflowContracts.WorkflowTaskCompleted(
            task.Id,
            task.WorkflowInstanceId,
            task.WorkflowStateId,
            action,
            task.CompletedAtUtc.Value));
        await _outboxRepository.AddAsync(OutboxMessage.Create("workflow.task.completed", payload), cancellationToken);
    }

    private async Task PublishDocumentLinkedAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        var linked = new DocumentLinked(
            null,
            instance.DocumentType,
            instance.DocumentId,
            "workflow.instance",
            instance.Id.ToString(),
            "workflows",
            DateTime.UtcNow);
        var payload = JsonSerializer.Serialize(linked);
        await _outboxRepository.AddAsync(OutboxMessage.Create("documents.linked", payload), cancellationToken);
    }

    private async Task PublishDocumentArchivedAsync(WorkflowInstance instance, CancellationToken cancellationToken)
    {
        var archived = new DocumentArchived(
            null,
            instance.DocumentType,
            instance.DocumentId,
            "workflows",
            DateTime.UtcNow,
            "workflow.completed");
        var payload = JsonSerializer.Serialize(archived);
        await _outboxRepository.AddAsync(OutboxMessage.Create("documents.archived", payload), cancellationToken);
    }

    private WorkflowTransition? FindTransition(WorkflowDefinition definition, long fromStateId, WorkflowActionType actionType, string? contextJson)
    {
        var transitions = definition.Transitions
            .Where(t => t.FromStateId == fromStateId && t.ActionType == actionType)
            .OrderBy(t => t.Id)
            .ToList();

        foreach (var transition in transitions)
        {
            if (_ruleEvaluator.Evaluate(transition.RuleJson, contextJson))
            {
                return transition;
            }
        }

        return null;
    }
}
