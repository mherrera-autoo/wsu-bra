namespace ERP.Modules.Rfid.Application.Commands;

public sealed record IngestPortalExitResult(bool Duplicated, int MovementsCreated, IReadOnlyCollection<string> UnmappedEpcs, string? Warning);
public sealed record PaginatedResult<T>(IReadOnlyList<T> Items, int Total);
