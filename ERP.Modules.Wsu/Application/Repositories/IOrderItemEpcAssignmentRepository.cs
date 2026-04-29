using ERP.Modules.Wsu.Domain;

namespace ERP.Modules.Wsu.Application.Repositories;

public sealed record OrderItemEpcCheckUpdateItem(
    string Epc,
    bool IsChecked);

public sealed record OrderItemEpcCheckUpdateResult(
    int UpdatedCount,
    int TotalAssignments,
    int CheckedAssignments,
    IReadOnlyList<string> NotFoundEpcs)
{
    public bool IsEpcSkuMatchCompleted => TotalAssignments > 0 && CheckedAssignments == TotalAssignments;
}

public sealed record OrderItemEpcAssignmentListItem(
    string? SkuWsu,
    string? NombreSkuWsu,
    string? SkuProveedor,
    string? NombreSkuProveedor,
    string? SkuCliente,
    string? NombreSkuCliente,
    string Epc,
    bool IsChecked);

public interface IOrderItemEpcAssignmentRepository
{
    Task<bool> AnyByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListEpcsByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);
    Task DeleteByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IReadOnlyCollection<OrderItemEpcAssignment> items, CancellationToken cancellationToken = default);
    Task<OrderItemEpcCheckUpdateResult> ApplyChecksAsync(long orderId, IReadOnlyCollection<OrderItemEpcCheckUpdateItem> items, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderItemEpcAssignmentListItem>> ListDetailedByOrderIdAsync(long orderId, CancellationToken cancellationToken = default);
}
