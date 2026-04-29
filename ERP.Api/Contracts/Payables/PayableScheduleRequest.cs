namespace ERP.Api.Contracts.Payables;

public sealed record PayableScheduleRequest(DateTime DueDate, decimal Amount);
