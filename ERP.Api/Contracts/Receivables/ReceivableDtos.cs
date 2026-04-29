namespace ERP.Api.Contracts.Receivables;

public sealed record ReceivableScheduleDto(DateTime DueDate, decimal Amount, string Status);

public sealed record ReceivableDto(
    long Id,
    long CustomerId,
    string Origin,
    string Status,
    string Currency,
    decimal TotalAmount,
    decimal OutstandingAmount,
    DateTime IssueDate,
    IReadOnlyCollection<ReceivableScheduleDto> Schedules);

public sealed record ReceivableAgingBucketDto(string Bucket, decimal Amount);

public sealed record ReceivableAgingResponse(
    long CompanyId,
    DateTime AsOf,
    IReadOnlyCollection<ReceivableAgingBucketDto> Buckets,
    decimal TotalOutstanding);
