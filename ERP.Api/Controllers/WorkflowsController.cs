using ERP.Api.Contracts.Workflows;
using ERP.Workflows.Application.Repositories;
using ERP.Workflows.Application.Services;
using ERP.Workflows.Domain;
using ERP.Shared.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[ApiController]
[Route("api/workflows")]
public sealed class WorkflowsController : ControllerBase
{
    private readonly WorkflowService _workflowService;
    private readonly IWorkflowInstanceRepository _workflowInstanceRepository;
    private readonly ICurrentUserProvider _currentUserProvider;

    public WorkflowsController(
        WorkflowService workflowService,
        IWorkflowInstanceRepository workflowInstanceRepository,
        ICurrentUserProvider currentUserProvider)
    {
        _workflowService = workflowService;
        _workflowInstanceRepository = workflowInstanceRepository;
        _currentUserProvider = currentUserProvider;
    }

    [HttpPost("definitions")]
    [Authorize]
    public async Task<IActionResult> CreateDefinition([FromBody] WorkflowDefinitionCreateRequest request, CancellationToken cancellationToken)
    {
        var definition = new WorkflowDefinition(request.Name, request.DocumentType);

        var stateMap = new Dictionary<string, WorkflowState>(StringComparer.OrdinalIgnoreCase);
        foreach (var stateRequest in request.States)
        {
            var state = definition.AddState(
                stateRequest.Name,
                stateRequest.IsInitial,
                stateRequest.IsFinal,
                stateRequest.RequiresApproval,
                stateRequest.AssignedRole,
                stateRequest.AssignedUserId);
            stateMap[state.Name] = state;
        }

        foreach (var transitionRequest in request.Transitions)
        {
            if (!stateMap.TryGetValue(transitionRequest.FromState, out var fromState))
            {
                return BadRequest($"State '{transitionRequest.FromState}' not found.");
            }

            if (!stateMap.TryGetValue(transitionRequest.ToState, out var toState))
            {
                return BadRequest($"State '{transitionRequest.ToState}' not found.");
            }

            if (!Enum.TryParse<WorkflowActionType>(transitionRequest.ActionType, true, out var actionType))
            {
                return BadRequest($"Invalid action type '{transitionRequest.ActionType}'.");
            }

            definition.AddTransition(fromState, toState, actionType, transitionRequest.RuleJson);
        }

        var result = await _workflowService.CreateDefinitionAsync(definition, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(new { definition.Id, definition.Name, definition.DocumentType });
    }

    [HttpPost("instances/start")]
    [Authorize]
    public async Task<IActionResult> StartWorkflow([FromBody] WorkflowStartRequest request, CancellationToken cancellationToken)
    {
        var performedBy = _currentUserProvider.GetCurrentUser()?.Email;
        var result = await _workflowService.StartWorkflowAsync(
            request.DocumentType,
            request.DocumentId,
            request.ContextDataJson,
            performedBy,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(new { result.Value!.Id, result.Value.DocumentId, result.Value.DocumentType, result.Value.Status });
    }

    [HttpPost("tasks/{taskId:long}/approve")]
    [Authorize]
    public async Task<IActionResult> ApproveTask(long taskId, [FromBody] WorkflowTaskActionRequest request, CancellationToken cancellationToken)
    {
        var performedBy = _currentUserProvider.GetCurrentUser()?.Email;
        var result = await _workflowService.ApproveTaskAsync(taskId, performedBy, request.Comment, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(new { result.Value!.Id, result.Value.Status, result.Value.CurrentStateId });
    }

    [HttpPost("tasks/{taskId:long}/reject")]
    [Authorize]
    public async Task<IActionResult> RejectTask(long taskId, [FromBody] WorkflowTaskActionRequest request, CancellationToken cancellationToken)
    {
        var performedBy = _currentUserProvider.GetCurrentUser()?.Email;
        var result = await _workflowService.RejectTaskAsync(taskId, performedBy, request.Comment, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok(new { result.Value!.Id, result.Value.Status, result.Value.CurrentStateId });
    }

    [HttpGet("instances/{instanceId:long}")]
    [Authorize]
    public async Task<IActionResult> GetInstance(long instanceId, CancellationToken cancellationToken)
    {
        var instance = await _workflowInstanceRepository.GetByIdAsync(instanceId, cancellationToken);
        if (instance is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            instance.Id,
            instance.DocumentType,
            instance.DocumentId,
            instance.Status,
            instance.CurrentStateId,
            instance.StartedAt,
            instance.CompletedAt
        });
    }

    [HttpGet("instances/{instanceId:long}/history")]
    [Authorize]
    public async Task<IActionResult> GetHistory(long instanceId, CancellationToken cancellationToken)
    {
        var history = await _workflowService.GetHistoryAsync(instanceId, cancellationToken);
        return Ok(history.Select(item => new
        {
            item.Id,
            item.ActionType,
            item.FromStateId,
            item.ToStateId,
            item.PerformedBy,
            item.PerformedAtUtc,
            item.Comment
        }));
    }
}
