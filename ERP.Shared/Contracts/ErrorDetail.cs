namespace ERP.Shared.Contracts;

public sealed class ErrorDetail
{
    public string? Field { get; set; }
    public string Message { get; set; } = string.Empty;
}
