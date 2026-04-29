using System.Text.Json;
using ERP.Modules.Integrations.Contracts;
using ERP.Modules.Sales.Application.Commands;
using ERP.Modules.Sales.Application.Repositories;
using ERP.Modules.Sales.Contracts;
using ERP.Modules.Sales.Domain;
using ERP.Shared.Application;
using DomainSalesQuoteStatus = ERP.Modules.Sales.Domain.SalesQuoteStatus;

namespace ERP.Modules.Sales.Application.Handlers;

public sealed class SubmitSalesQuoteHandler
{
    private readonly ISalesQuoteRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitSalesQuoteHandler(
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

    public Task<Result<SalesQuote>> HandleAsync(SubmitSalesQuoteCommand command, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var companyId = _tenantContext.CompanyId ?? 0;
            if (companyId <= 0)
            {
                return Result<SalesQuote>.Fail("Tenant context is missing CompanyId.");
            }

            var quote = await _repository.GetByPublicIdAsync(companyId, command.QuotePublicId, token);
            if (quote is null)
            {
                return Result<SalesQuote>.Fail("Sales quote not found.");
            }

            if (quote.Status == DomainSalesQuoteStatus.Submitted)
            {
                return Result<SalesQuote>.Ok(quote);
            }

            if (quote.Status != DomainSalesQuoteStatus.Draft)
            {
                return Result<SalesQuote>.Fail("Sales quote status conflict.");
            }

            quote.Submit(_tenantContext.UserId);

            var submittedEvent = new SalesQuoteSubmitted(quote.CompanyId, quote.PublicId);
            var payload = JsonSerializer.Serialize(submittedEvent);
            await _outboxRepository.AddAsync(OutboxMessage.Create("sales.quote.submitted", payload), token);

            return Result<SalesQuote>.Ok(quote);
        }, cancellationToken);
    }
}

public sealed class ApproveSalesQuoteHandler
{
    private readonly ISalesQuoteRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveSalesQuoteHandler(
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

    public Task<Result<SalesQuote>> HandleAsync(ApproveSalesQuoteCommand command, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var companyId = _tenantContext.CompanyId ?? 0;
            if (companyId <= 0)
            {
                return Result<SalesQuote>.Fail("Tenant context is missing CompanyId.");
            }

            var quote = await _repository.GetByPublicIdAsync(companyId, command.QuotePublicId, token);
            if (quote is null)
            {
                return Result<SalesQuote>.Fail("Sales quote not found.");
            }

            if (quote.Status == DomainSalesQuoteStatus.Approved)
            {
                return Result<SalesQuote>.Ok(quote);
            }

            if (quote.Status != DomainSalesQuoteStatus.Submitted)
            {
                return Result<SalesQuote>.Fail("Sales quote status conflict.");
            }

            quote.Approve(_tenantContext.UserId);

            var approvedEvent = new SalesQuoteApproved(quote.CompanyId, quote.PublicId);
            var payload = JsonSerializer.Serialize(approvedEvent);
            await _outboxRepository.AddAsync(OutboxMessage.Create("sales.quote.approved", payload), token);

            return Result<SalesQuote>.Ok(quote);
        }, cancellationToken);
    }
}

public sealed class RejectSalesQuoteHandler
{
    private readonly ISalesQuoteRepository _repository;
    private readonly ITenantContext _tenantContext;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectSalesQuoteHandler(
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

    public Task<Result<SalesQuote>> HandleAsync(RejectSalesQuoteCommand command, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.ExecuteInTransactionAsync(async token =>
        {
            var companyId = _tenantContext.CompanyId ?? 0;
            if (companyId <= 0)
            {
                return Result<SalesQuote>.Fail("Tenant context is missing CompanyId.");
            }

            if (string.IsNullOrWhiteSpace(command.Reason))
            {
                return Result<SalesQuote>.Fail("Reject reason is required.");
            }

            var quote = await _repository.GetByPublicIdAsync(companyId, command.QuotePublicId, token);
            if (quote is null)
            {
                return Result<SalesQuote>.Fail("Sales quote not found.");
            }

            if (quote.Status == DomainSalesQuoteStatus.Rejected)
            {
                return Result<SalesQuote>.Ok(quote);
            }

            if (quote.Status != DomainSalesQuoteStatus.Submitted)
            {
                return Result<SalesQuote>.Fail("Sales quote status conflict.");
            }

            quote.Reject(_tenantContext.UserId);

            var rejectedEvent = new SalesQuoteRejected(quote.CompanyId, quote.PublicId, command.Reason.Trim());
            var payload = JsonSerializer.Serialize(rejectedEvent);
            await _outboxRepository.AddAsync(OutboxMessage.Create("sales.quote.rejected", payload), token);

            return Result<SalesQuote>.Ok(quote);
        }, cancellationToken);
    }
}
