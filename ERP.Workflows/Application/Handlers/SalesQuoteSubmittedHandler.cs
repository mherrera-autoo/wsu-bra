using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Sales.Contracts;
using ERP.Workflows.Application.Services;

namespace ERP.Workflows.Application.Handlers;

public sealed class SalesQuoteSubmittedHandler : IOutboxMessageHandler
{
    private readonly WorkflowService _workflowService;

    public SalesQuoteSubmittedHandler(WorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    public string MessageType => "sales.quote.submitted";

    public async Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        var submitted = JsonSerializer.Deserialize<SalesQuoteSubmitted>(payloadJson);
        if (submitted is null)
        {
            throw new InvalidOperationException("Invalid sales quote submitted payload.");
        }

        var result = await _workflowService.StartWorkflowAsync(
            "SalesQuote",
            submitted.QuotePublicId.ToString(),
            null,
            "sales",
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "No active workflow definition for the document type.", StringComparison.OrdinalIgnoreCase)
                || string.Equals(result.Error, "Workflow already exists for the document.", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new InvalidOperationException(result.Error ?? "Failed to start workflow for sales quote.");
        }
    }
}
