using System.Reflection;
using ERP.Workflows.Application.Repositories;
using ERP.Workflows.Application.Services;
using ERP.Workflows.Domain;
using ERP.Shared.Application;
using ERP.Modules.Integrations.Contracts;
using Xunit;

namespace ERP.Workflows.WhiteBox.Tests;

public sealed class WorkflowServiceTests
{
    [Fact]
    public async Task StartWorkflow_AutoTransitionsToFinalState()
    {
        var definitionRepository = new InMemoryWorkflowDefinitionRepository();
        var instanceRepository = new InMemoryWorkflowInstanceRepository();
        var taskRepository = new InMemoryWorkflowTaskRepository();
        var historyRepository = new InMemoryWorkflowHistoryRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var service = new WorkflowService(
            definitionRepository,
            instanceRepository,
            taskRepository,
            historyRepository,
            new InMemoryOutboxRepository(),
            new WorkflowRuleEvaluator(),
            unitOfWork);

        var definition = new WorkflowDefinition("PO Approval", "PurchaseOrder");
        var draft = definition.AddState("Draft", true, false, false, null, null);
        var approved = definition.AddState("Approved", false, true, false, null, null);
        definition.AddTransition(draft, approved, WorkflowActionType.Auto, "{\"all\":[{\"field\":\"totalAmount\",\"op\":\"<\",\"value\":1000}]} ");

        await service.CreateDefinitionAsync(definition);

        var result = await service.StartWorkflowAsync(
            "PurchaseOrder",
            "PO-1",
            "{\"totalAmount\":500}",
            "tester");

        Assert.True(result.Success, result.Error);
        Assert.Equal(WorkflowInstanceStatus.Completed, result.Value!.Status);
    }

    [Fact]
    public async Task ApproveTask_MovesToNextState()
    {
        var definitionRepository = new InMemoryWorkflowDefinitionRepository();
        var instanceRepository = new InMemoryWorkflowInstanceRepository();
        var taskRepository = new InMemoryWorkflowTaskRepository();
        var historyRepository = new InMemoryWorkflowHistoryRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var service = new WorkflowService(
            definitionRepository,
            instanceRepository,
            taskRepository,
            historyRepository,
            new InMemoryOutboxRepository(),
            new WorkflowRuleEvaluator(),
            unitOfWork);

        var definition = new WorkflowDefinition("PO Approval", "PurchaseOrder");
        var draft = definition.AddState("Draft", true, false, true, "Finance", null);
        var approved = definition.AddState("Approved", false, true, false, null, null);
        definition.AddTransition(draft, approved, WorkflowActionType.Approve, null);
        await service.CreateDefinitionAsync(definition);

        var startResult = await service.StartWorkflowAsync(
            "PurchaseOrder",
            "PO-2",
            "{\"totalAmount\":1500}",
            "tester");

        Assert.True(startResult.Success, startResult.Error);
        var instance = startResult.Value!;
        var task = taskRepository.Tasks.Single();

        var approveResult = await service.ApproveTaskAsync(task.Id, "approver", "ok");
        Assert.True(approveResult.Success, approveResult.Error);
        Assert.Equal(WorkflowInstanceStatus.Completed, approveResult.Value!.Status);
    }

    private sealed class InMemoryUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(0);

        public Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> action, CancellationToken cancellationToken = default)
            => action(cancellationToken);

        public Task<Result<T>> ExecuteInTransactionAsync<T>(Func<CancellationToken, Task<Result<T>>> action, CancellationToken cancellationToken = default)
            => action(cancellationToken);
    }

    private sealed class InMemoryWorkflowDefinitionRepository : IWorkflowDefinitionRepository
    {
        private readonly List<WorkflowDefinition> _definitions = new();
        private long _nextId = 1;

        public Task AddAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
        {
            SetEntityId(definition, _nextId++);
            foreach (var state in definition.States)
            {
                SetEntityId(state, _nextId++);
                SetProperty(state, "WorkflowDefinitionId", definition.Id);
            }

            foreach (var transition in definition.Transitions)
            {
                SetEntityId(transition, _nextId++);
                SetProperty(transition, "WorkflowDefinitionId", definition.Id);
                SetProperty(transition, "FromStateId", transition.FromState?.Id ?? 0);
                SetProperty(transition, "ToStateId", transition.ToState?.Id ?? 0);
            }

            _definitions.Add(definition);
            return Task.CompletedTask;
        }

        public Task<WorkflowDefinition?> GetActiveByDocumentTypeAsync(string documentType, CancellationToken cancellationToken = default)
            => Task.FromResult(_definitions.FirstOrDefault(d => d.DocumentType == documentType && d.IsActive));

        public Task<WorkflowDefinition?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => Task.FromResult(_definitions.FirstOrDefault(d => d.Id == id));
    }

    private sealed class InMemoryWorkflowInstanceRepository : IWorkflowInstanceRepository
    {
        private readonly List<WorkflowInstance> _instances = new();
        private long _nextId = 1;

        public Task AddAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
        {
            SetEntityId(instance, _nextId++);
            _instances.Add(instance);
            return Task.CompletedTask;
        }

        public Task<WorkflowInstance?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => Task.FromResult(_instances.FirstOrDefault(i => i.Id == id));

        public Task<WorkflowInstance?> GetByDocumentAsync(string documentType, string documentId, CancellationToken cancellationToken = default)
            => Task.FromResult(_instances.FirstOrDefault(i => i.DocumentType == documentType && i.DocumentId == documentId));

        public Task UpdateAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryWorkflowTaskRepository : IWorkflowTaskRepository
    {
        private readonly List<WorkflowTask> _tasks = new();
        private long _nextId = 1;

        public IReadOnlyList<WorkflowTask> Tasks => _tasks;

        public Task AddAsync(WorkflowTask task, CancellationToken cancellationToken = default)
        {
            SetEntityId(task, _nextId++);
            _tasks.Add(task);
            return Task.CompletedTask;
        }

        public Task<WorkflowTask?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
            => Task.FromResult(_tasks.FirstOrDefault(t => t.Id == id));

        public Task<IReadOnlyList<WorkflowTask>> GetPendingByInstanceAsync(long workflowInstanceId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<WorkflowTask>>(_tasks.Where(t => t.WorkflowInstanceId == workflowInstanceId && t.Status == WorkflowTaskStatus.Pending).ToList());

        public Task UpdateAsync(WorkflowTask task, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryWorkflowHistoryRepository : IWorkflowHistoryRepository
    {
        private readonly List<WorkflowHistory> _history = new();
        private long _nextId = 1;

        public Task AddAsync(WorkflowHistory history, CancellationToken cancellationToken = default)
        {
            SetEntityId(history, _nextId++);
            _history.Add(history);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<WorkflowHistory>> GetByInstanceAsync(long workflowInstanceId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<WorkflowHistory>>(_history.Where(h => h.WorkflowInstanceId == workflowInstanceId).ToList());
    }

    private sealed class InMemoryOutboxRepository : IOutboxRepository
    {
        public List<OutboxMessage> Messages { get; } = new();

        public Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(
            int batchSize,
            DateTime utcNow,
            int maxAttempts,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<OutboxMessage>>(Messages);

        public Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private static void SetEntityId(object entity, long id)
    {
        var property = entity.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        property?.SetValue(entity, id);
    }

    private static void SetProperty(object entity, string name, object? value)
    {
        var property = entity.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        property?.SetValue(entity, value);
    }
}
