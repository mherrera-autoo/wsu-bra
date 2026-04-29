namespace ERP.Api.Contracts.Workflows;

public sealed record WorkflowStartRequest(
    string DocumentType,
    string DocumentId,
    string? ContextDataJson);

public sealed record WorkflowTaskActionRequest(string? Comment);
