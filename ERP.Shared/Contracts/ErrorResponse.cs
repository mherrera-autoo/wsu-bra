namespace ERP.Shared.Contracts;

public sealed class ErrorResponse
{
    public string TraceId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public List<ErrorDetail>? Errors { get; set; }
    public string TimestampUtc { get; set; } = string.Empty;
}
