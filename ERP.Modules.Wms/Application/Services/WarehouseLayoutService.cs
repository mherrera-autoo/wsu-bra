using ERP.Modules.Wms.Application.Repositories;
using ERP.Modules.Wms.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.Wms.Application.Services;

public sealed class WarehouseLayoutService
{
    private readonly IWarehouseLocationRepository _locationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WarehouseLayoutService(
        IWarehouseLocationRepository locationRepository,
        IUnitOfWork unitOfWork)
    {
        _locationRepository = locationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int>> CreateStandardLayoutAsync(
        long companyId,
        long warehouseId,
        int aisleCount,
        int racksPerAisle,
        int sidesPerRack,
        int levelsPerRack,
        int positionsPerLevel,
        decimal palletSlotCapacity,
        CancellationToken cancellationToken = default)
    {
        if (aisleCount <= 0 || racksPerAisle <= 0 || sidesPerRack <= 0 || levelsPerRack <= 0 || positionsPerLevel <= 0)
        {
            return Result<int>.Fail("Layout dimensions must be greater than zero.");
        }

        var created = 0;
        for (var aisle = 1; aisle <= aisleCount; aisle++)
        {
            for (var rack = 1; rack <= racksPerAisle; rack++)
            {
                for (var side = 1; side <= sidesPerRack; side++)
                {
                    for (var level = 1; level <= levelsPerRack; level++)
                    {
                        for (var slot = 1; slot <= positionsPerLevel; slot++)
                        {
                            var location = WarehouseLocation.Create(
                                companyId,
                                warehouseId,
                                $"A{aisle}",
                                $"R{rack}",
                                $"S{side}",
                                level,
                                slot,
                                palletSlotCapacity,
                                isPalletSlot: true,
                                isStackable: false,
                                notes: null);

                            await _locationRepository.AddAsync(location, cancellationToken);
                            created++;
                        }
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<int>.Ok(created);
    }
}
