using System;
using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Application.Services;
using ERP.Modules.Sales.Contracts;
using ERP.Modules.Sales.Application.Repositories;
using ERP.Modules.Sales.Domain;
using ERP.Modules.Tax.Contracts;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;
using SalesDocumentKind = ERP.Modules.Sales.Domain.SalesDocumentKind;

namespace ERP.Modules.Sales.Application.Services;

public sealed class SalesService
{
    private const string MissingCurrencyMessage = "Currency is required.";
    private readonly ISalesDocumentRepository _salesDocumentRepository;
    private readonly ITaxCalculationQuery _taxCalculationQuery;
    private readonly ICompanyCurrencyValidationService _companyCurrencyValidationService;
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outboxRepository;

    public SalesService(
        ISalesDocumentRepository salesDocumentRepository,
        ITaxCalculationQuery taxCalculationQuery,
        ICompanyCurrencyValidationService companyCurrencyValidationService,
        ICurrencyRepository currencyRepository,
        IUnitOfWork unitOfWork,
        IOutboxRepository outboxRepository)
    {
        _salesDocumentRepository = salesDocumentRepository;
        _taxCalculationQuery = taxCalculationQuery;
        _companyCurrencyValidationService = companyCurrencyValidationService;
        _currencyRepository = currencyRepository;
        _unitOfWork = unitOfWork;
        _outboxRepository = outboxRepository;
    }

    public async Task<Result<SalesDocument>> CreateQuoteAsync(
        long companyId,
        long customerId,
        IEnumerable<(long productId, decimal qty, Money unitPrice, long? taxGroupId)> lines,
        CancellationToken cancellationToken = default)
    {
        var lineItems = lines.ToList();
        var currencyValidation = await ValidateDocumentCurrencyAsync(companyId, lineItems, cancellationToken);
        if (!currencyValidation.Success)
        {
            return Result<SalesDocument>.Fail(currencyValidation.Error ?? MissingCurrencyMessage);
        }

        var document = SalesDocument.Create(companyId, SalesDocumentKind.Quote, customerId);
        foreach (var (productId, qty, unitPrice, taxGroupId) in lineItems)
        {
            var baseAmount = qty * unitPrice.Amount;
            var taxResult = await _taxCalculationQuery.CalculateTaxAmountAsync(companyId, taxGroupId, baseAmount, unitPrice.Currency, cancellationToken);
            if (!taxResult.Success)
            {
                return Result<SalesDocument>.Fail(taxResult.Error ?? "Failed to calculate taxes.");
            }

            document.AddLine(productId, qty, unitPrice, taxGroupId, taxResult.Value);
        }

        await _salesDocumentRepository.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SalesDocument>.Ok(document);
    }

    public async Task<Result<SalesDocument>> CreateOrderAsync(
        long companyId,
        long customerId,
        IEnumerable<(long productId, decimal qty, Money unitPrice, long? taxGroupId)> lines,
        CancellationToken cancellationToken = default)
    {
        var lineItems = lines.ToList();
        var currencyValidation = await ValidateDocumentCurrencyAsync(companyId, lineItems, cancellationToken);
        if (!currencyValidation.Success)
        {
            return Result<SalesDocument>.Fail(currencyValidation.Error ?? MissingCurrencyMessage);
        }

        var document = SalesDocument.Create(companyId, SalesDocumentKind.Order, customerId);
        foreach (var (productId, qty, unitPrice, taxGroupId) in lineItems)
        {
            var baseAmount = qty * unitPrice.Amount;
            var taxResult = await _taxCalculationQuery.CalculateTaxAmountAsync(companyId, taxGroupId, baseAmount, unitPrice.Currency, cancellationToken);
            if (!taxResult.Success)
            {
                return Result<SalesDocument>.Fail(taxResult.Error ?? "Failed to calculate taxes.");
            }

            document.AddLine(productId, qty, unitPrice, taxGroupId, taxResult.Value);
        }

        await _salesDocumentRepository.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<SalesDocument>.Ok(document);
    }

    public async Task<Result<SalesDocument>> ApproveSalesOrderAsync(
        long documentId,
        long companyId,
        long? warehouseId,
        CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var document = await _salesDocumentRepository.GetByIdAsync(documentId, token);
            if (document is null || document.CompanyId != companyId)
            {
                return Result<SalesDocument>.Fail("Sales document not found.");
            }

            document.Approve();

            if (document.Kind is SalesDocumentKind.Order or SalesDocumentKind.Invoice)
            {
                if (warehouseId is null)
                {
                    return Result<SalesDocument>.Fail("WarehouseId is required to ship stock.");
                }

                var shipment = new SalesShipmentRequested(
                    document.CompanyId,
                    document.Id,
                    document.CustomerId,
                    warehouseId.Value,
                    document.Lines.Select(line => new SalesShipmentLine(line.ProductId, line.Qty, Array.Empty<SalesShipmentBatchInput>())).ToList());
                var payload = JsonSerializer.Serialize(shipment);
                await _outboxRepository.AddAsync(
                    OutboxMessage.Create("sales.shipment.requested", payload),
                    token);
            }

            if (document.Kind is SalesDocumentKind.Invoice or SalesDocumentKind.CreditNote)
            {
                var (totalAmount, currency) = document.CalculateTotal();
                var issueDate = DateTime.UtcNow.Date;
                var dueDate = document.Kind == SalesDocumentKind.Invoice
                    ? issueDate.AddDays(30)
                    : (DateTime?)null;

                var receivable = new SalesReceivableRequested(
                    document.CompanyId,
                    document.Id,
                    document.CustomerId,
                    document.Kind.ToString(),
                    totalAmount,
                    currency,
                    issueDate,
                    dueDate);
                var payload = JsonSerializer.Serialize(receivable);
                await _outboxRepository.AddAsync(
                    OutboxMessage.Create("sales.receivable.requested", payload),
                    token);
            }

            return Result<SalesDocument>.Ok(document);
        }, cancellationToken);
    }

    public Task<IReadOnlyList<SalesDocument>> ListDocumentsAsync(long companyId, CancellationToken cancellationToken = default)
        => _salesDocumentRepository.ListAsync(companyId, cancellationToken);

    public async Task<SalesDocument?> GetDocumentAsync(long companyId, long id, CancellationToken cancellationToken = default)
    {
        var document = await _salesDocumentRepository.GetByIdAsync(id, cancellationToken);
        return document?.CompanyId == companyId ? document : null;
    }

    private async Task<Result> ValidateDocumentCurrencyAsync(
        long companyId,
        IReadOnlyCollection<(long productId, decimal qty, Money unitPrice, long? taxGroupId)> lines,
        CancellationToken cancellationToken)
    {
        if (lines.Count == 0)
        {
            return Result.Fail("At least one line is required.");
        }

        var currencies = lines
            .Select(line => line.unitPrice.Currency)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (currencies.Count != 1)
        {
            return Result.Fail("All sales document lines must use the same currency.");
        }

        var currencyCode = currencies[0];
        var currencyEntity = await _currencyRepository.GetByCodeAsync(currencyCode, cancellationToken);
        if (currencyEntity is null)
        {
            return Result.Fail($"Currency code '{currencyCode}' was not found.");
        }

        var validationResult = await _companyCurrencyValidationService.ValidateCurrencyForCompanyAsync(companyId, currencyEntity.Id, cancellationToken);
        if (!validationResult.Success)
        {
            return Result.Fail(validationResult.Error ?? "Currency is not allowed for the company.");
        }

        return Result.Ok();
    }
}
