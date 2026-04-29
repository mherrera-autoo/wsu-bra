using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Sales.Application.Commands;
using ERP.Modules.Sales.Application.Repositories;
using ERP.Modules.Sales.Contracts;
using ERP.Modules.Sales.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Sales.Application.Handlers;

public sealed class CreateSalesQuoteHandler
{
    private readonly ISalesQuoteRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSalesQuoteHandler(
        ISalesQuoteRepository repository,
        ITenantContext tenantContext,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _tenantContext = tenantContext;
        _outboxRepository = outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<SalesQuote>> HandleAsync(CreateSalesQuoteCommand command, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var companyId = _tenantContext.CompanyId ?? 0;
            if (companyId <= 0)
            {
                return Result<SalesQuote>.Fail("Tenant context is missing CompanyId.");
            }

            if (string.IsNullOrWhiteSpace(command.CustomerName))
            {
                return Result<SalesQuote>.Fail("Customer name is required.");
            }

            if (command.Lines.Count == 0)
            {
                return Result<SalesQuote>.Fail("At least one line is required.");
            }

            var quoteNumber = await _repository.GetNextQuoteNumberAsync(companyId, token);
            var userId = _tenantContext.UserId;
            var quote = SalesQuote.Create(companyId, quoteNumber, command.CustomerName, command.Notes, command.CurrencyCode, userId);

            var lineNumber = 1;
            foreach (var line in command.Lines)
            {
                quote.AddLine(
                    lineNumber++,
                    line.ProductId,
                    line.Description,
                    line.Quantity,
                    line.UnitPrice,
                    line.TaxRate);
            }

            await _repository.AddAsync(quote, token);

            var createdEvent = new SalesQuoteCreated(quote.CompanyId, quote.PublicId);
            var payload = JsonSerializer.Serialize(createdEvent);
            await _outboxRepository.AddAsync(OutboxMessage.Create("sales.quote.created", payload), token);

            return Result<SalesQuote>.Ok(quote);
        }, cancellationToken);
    }
}
