using System.Text.Json;
using ERP.Documents.Contracts;
using ERP.Modules.Integrations.Contracts;
using ERP.Workflows.Application.Services;

namespace ERP.Workflows.Application.Handlers;

public sealed class DocumentUploadedHandler : IOutboxMessageHandler
{
    private readonly WorkflowService _workflowService;

    public DocumentUploadedHandler(WorkflowService workflowService)
    {
        _workflowService = workflowService;
    }

    public string MessageType => "documents.uploaded";

    public async Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        var uploaded = JsonSerializer.Deserialize<DocumentUploaded>(payloadJson);
        if (uploaded is null)
        {
            throw new InvalidOperationException("Invalid document uploaded payload.");
        }

        var result = await _workflowService.StartWorkflowAsync(
            uploaded.DocumentType,
            uploaded.DocumentId,
            uploaded.ContextDataJson,
            uploaded.UploadedBy ?? uploaded.SourceModule,
            cancellationToken);

        if (!result.Success)
        {
            if (string.Equals(result.Error, "No active workflow definition for the document type.", StringComparison.OrdinalIgnoreCase)
                || string.Equals(result.Error, "Workflow already exists for the document.", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new InvalidOperationException(result.Error ?? "Failed to start workflow for document.");
        }
    }
}
