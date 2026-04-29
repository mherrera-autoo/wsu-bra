using System.Text.Json;
using ERP.Modules.Accounting.Application.Services;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Sales.Contracts;
using ERP.Shared.Application;

namespace ERP.Modules.Accounting.Application.Handlers;

public sealed class SalesReceivableRequestedHandler : IOutboxMessageHandler
{
    private readonly ReceivablesService _receivablesService;
    private readonly IUnitOfWork _unitOfWork;

    public SalesReceivableRequestedHandler(ReceivablesService receivablesService, IUnitOfWork unitOfWork)
    {
        _receivablesService = receivablesService;
        _unitOfWork = unitOfWork;
    }

    public string MessageType => "sales.receivable.requested";

    public async Task HandleAsync(string payloadJson, CancellationToken cancellationToken = default)
    {
        var request = JsonSerializer.Deserialize<SalesReceivableRequested>(payloadJson);
        if (request is null)
        {
            throw new InvalidOperationException("Invalid sales receivable payload.");
        }

        var result = await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            if (string.Equals(request.DocumentKind, "Invoice", StringComparison.OrdinalIgnoreCase))
            {
                var dueDate = request.DueDate ?? request.IssueDate;
                var receivableResult = await _receivablesService.CreateFromInvoiceAsync(
                    request.CompanyId,
                    request.CustomerId,
                    request.DocumentId,
                    request.IssueDate,
                    dueDate,
                    request.Currency,
                    request.TotalAmount,
                    token);

                return receivableResult.Success
                    ? Result.Ok()
                    : Result.Fail(receivableResult.Error ?? "Failed to create accounts receivable.");
            }

            if (string.Equals(request.DocumentKind, "CreditNote", StringComparison.OrdinalIgnoreCase))
            {
                var receivableResult = await _receivablesService.CreateFromCreditNoteAsync(
                    request.CompanyId,
                    request.CustomerId,
                    request.DocumentId,
                    request.IssueDate,
                    request.Currency,
                    request.TotalAmount,
                    token);

                return receivableResult.Success
                    ? Result.Ok()
                    : Result.Fail(receivableResult.Error ?? "Failed to create credit note receivable.");
            }

            return Result.Fail($"Unsupported sales document kind: {request.DocumentKind}.");
        }, cancellationToken);

        if (!result.Success)
        {
            throw new InvalidOperationException(result.Error ?? "Failed to create sales receivable.");
        }
    }
}
