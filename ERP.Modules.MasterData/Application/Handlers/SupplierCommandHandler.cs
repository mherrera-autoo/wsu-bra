using ERP.Modules.MasterData.Application.Commands;
using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Handlers;

public sealed class SupplierCommandHandler
{
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SupplierCommandHandler(ISupplierRepository supplierRepository, IUnitOfWork unitOfWork)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Supplier>> HandleAsync(CreateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<Supplier>.Fail("Supplier name is required.");
        }

        if (await _supplierRepository.ExistsByNameAsync(command.CompanyId, command.Name.Trim(), cancellationToken: cancellationToken))
        {
            return Result<Supplier>.Fail($"Supplier '{command.Name}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(command.TaxId))
        {
            var taxId = command.TaxId.Trim();
            if (await _supplierRepository.ExistsByTaxIdAsync(command.CompanyId, taxId, cancellationToken: cancellationToken))
            {
                return Result<Supplier>.Fail($"Supplier tax id '{taxId}' already exists.");
            }
        }

        var supplier = Supplier.Create(command.CompanyId, command.Name, command.TaxId, command.Country, command.Currency);
        await _supplierRepository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Supplier>.Ok(supplier);
    }

    public async Task<Result<Supplier>> HandleAsync(UpdateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Result<Supplier>.Fail("Supplier name is required.");
        }

        var supplier = await _supplierRepository.GetByIdAsync(command.CompanyId, command.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return Result<Supplier>.Fail("Supplier not found.");
        }

        if (await _supplierRepository.ExistsByNameAsync(command.CompanyId, command.Name.Trim(), supplier.Id, cancellationToken))
        {
            return Result<Supplier>.Fail($"Supplier '{command.Name}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(command.TaxId))
        {
            var taxId = command.TaxId.Trim();
            if (await _supplierRepository.ExistsByTaxIdAsync(command.CompanyId, taxId, supplier.Id, cancellationToken))
            {
                return Result<Supplier>.Fail($"Supplier tax id '{taxId}' already exists.");
            }
        }

        supplier.Update(command.Name, command.TaxId, command.Country, command.Currency, command.IsActive);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Supplier>.Ok(supplier);
    }

    public async Task<Result> HandleAsync(DeleteSupplierCommand command, CancellationToken cancellationToken = default)
    {
        var supplier = await _supplierRepository.GetByIdAsync(command.CompanyId, command.SupplierId, cancellationToken);
        if (supplier is null)
        {
            return Result.Fail("Supplier not found.");
        }

        supplier.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
