using ERP.Modules.Pricing.Application.Repositories;
using ERP.Modules.Pricing.Domain;
using ERP.Shared.Application;
using ERP.Shared.Domain.ValueObjects;

namespace ERP.Modules.Pricing.Application.Services;

public sealed class PricingService
{
    private readonly IPriceListRepository _priceListRepository;
    private readonly IPriceListItemRepository _priceListItemRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PricingService(
        IPriceListRepository priceListRepository,
        IPriceListItemRepository priceListItemRepository,
        IUnitOfWork unitOfWork)
    {
        _priceListRepository = priceListRepository;
        _priceListItemRepository = priceListItemRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PriceList>> CreateDefaultPriceListAsync(
        long companyId,
        string name,
        CancellationToken cancellationToken = default)
    {
        var existing = await _priceListRepository.GetDefaultAsync(companyId, cancellationToken);
        if (existing is not null)
        {
            return Result<PriceList>.Ok(existing);
        }

        var priceList = PriceList.Create(companyId, name, true);
        await _priceListRepository.AddAsync(priceList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PriceList>.Ok(priceList);
    }

    public async Task<Result<PriceList>> CreatePriceListAsync(
        long companyId,
        string name,
        bool isDefault,
        CancellationToken cancellationToken = default)
    {
        if (await _priceListRepository.ExistsByNameAsync(companyId, name, cancellationToken))
        {
            return Result<PriceList>.Fail($"Price list name '{name}' already exists.");
        }

        if (isDefault)
        {
            var existingDefault = await _priceListRepository.GetDefaultAsync(companyId, cancellationToken);
            if (existingDefault is not null)
            {
                return Result<PriceList>.Fail("Default price list already exists.");
            }
        }

        var priceList = PriceList.Create(companyId, name, isDefault);
        await _priceListRepository.AddAsync(priceList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PriceList>.Ok(priceList);
    }

    public Task<PriceList?> GetPriceListAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _priceListRepository.GetByIdAsync(companyId, id, cancellationToken);

    public Task<IReadOnlyList<PriceList>> ListPriceListsAsync(long companyId, CancellationToken cancellationToken = default)
        => _priceListRepository.ListAsync(companyId, cancellationToken);

    public async Task<Result> DeletePriceListAsync(long companyId, long id, CancellationToken cancellationToken = default)
    {
        var priceList = await _priceListRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (priceList is null)
        {
            return Result.Fail("Price list not found.");
        }

        var items = await _priceListItemRepository.ListByPriceListAsync(companyId, id, cancellationToken);
        foreach (var item in items)
        {
            _priceListItemRepository.Remove(item);
        }

        _priceListRepository.Remove(priceList);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result<PriceListItem>> AddPriceAsync(
        long companyId,
        long priceListId,
        long productId,
        Money unitPrice,
        CancellationToken cancellationToken = default)
    {
        var priceList = await _priceListRepository.GetByIdAsync(companyId, priceListId, cancellationToken);
        if (priceList is null)
        {
            return Result<PriceListItem>.Fail("Price list not found.");
        }

        if (await _priceListItemRepository.ExistsAsync(companyId, priceListId, productId, cancellationToken))
        {
            return Result<PriceListItem>.Fail("Price list item already exists.");
        }

        var priceListItem = PriceListItem.Create(companyId, priceListId, productId, unitPrice);
        await _priceListItemRepository.AddAsync(priceListItem, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<PriceListItem>.Ok(priceListItem);
    }

    public Task<IReadOnlyList<PriceListItem>> ListPriceListItemsAsync(
        long companyId,
        long priceListId,
        CancellationToken cancellationToken = default)
        => _priceListItemRepository.ListByPriceListAsync(companyId, priceListId, cancellationToken);

    public async Task<Result> RemovePriceAsync(
        long companyId,
        long priceListId,
        long productId,
        CancellationToken cancellationToken = default)
    {
        var priceListItem = await _priceListItemRepository.GetByProductAsync(
            companyId,
            priceListId,
            productId,
            cancellationToken);
        if (priceListItem is null)
        {
            return Result.Fail("Price list item not found.");
        }

        _priceListItemRepository.Remove(priceListItem);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> UpdatePriceAsync(
        long companyId,
        long priceListId,
        long productId,
        Money unitPrice,
        CancellationToken cancellationToken = default)
    {
        var priceListItem = await _priceListItemRepository.GetByProductAsync(
            companyId,
            priceListId,
            productId,
            cancellationToken);
        if (priceListItem is null)
        {
            return Result.Fail("Price list item not found.");
        }

        priceListItem.UpdatePrice(unitPrice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
