using ERP.Modules.MasterData.Application.Repositories;
using ERP.Modules.MasterData.Domain;
using ERP.Shared.Application;

namespace ERP.Modules.MasterData.Application.Services;

public sealed class MasterDataService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfMeasureAccessValidator _unitOfMeasureAccessValidator;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MasterDataService(
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        ISupplierRepository supplierRepository,
        IUnitOfMeasureAccessValidator unitOfMeasureAccessValidator,
        IWarehouseRepository warehouseRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _supplierRepository = supplierRepository;
        _unitOfMeasureAccessValidator = unitOfMeasureAccessValidator;
        _warehouseRepository = warehouseRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Product>> CreateProductAsync(
        long companyId,
        string sku,
        string name,
        string? barcode,
        long unitOfMeasureId,
        bool isStockable,
        bool isSellable = true,
        bool isPurchasable = true,
        CancellationToken cancellationToken = default,
        bool? isStackable = null,
        decimal? lengthCm = null,
        decimal? widthCm = null,
        decimal? weightKg = null,
        string? storageType = null)
    {
        if (await _productRepository.ExistsBySkuAsync(companyId, sku, cancellationToken: cancellationToken))
        {
            return Result<Product>.Fail($"SKU '{sku}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(barcode)
            && await _productRepository.ExistsByBarcodeAsync(companyId, barcode, cancellationToken: cancellationToken))
        {
            return Result<Product>.Fail($"Barcode '{barcode}' already exists.");
        }

        if (!await _unitOfMeasureAccessValidator.IsEnabledForCompanyAsync(companyId, unitOfMeasureId, cancellationToken))
        {
            return Result<Product>.Fail("Unit of measure is not enabled for the company.");
        }

        var product = Product.Create(
            companyId,
            sku,
            name,
            barcode,
            unitOfMeasureId,
            isStockable,
            isSellable,
            isPurchasable,
            isStackable,
            lengthCm,
            widthCm,
            weightKg,
            storageType);
        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Product>.Ok(product);
    }

    public async Task<Result<Customer>> CreateCustomerAsync(
        long companyId,
        string name,
        string? taxId,
        CancellationToken cancellationToken = default)
    {
        if (await _customerRepository.ExistsByNameAsync(companyId, name, cancellationToken: cancellationToken))
        {
            return Result<Customer>.Fail($"Customer '{name}' already exists.");
        }

        var customer = Customer.Create(companyId, name, taxId);
        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Customer>.Ok(customer);
    }

    public async Task<Result<Supplier>> CreateSupplierAsync(
        long companyId,
        string name,
        string? taxId,
        string? country,
        string? currency,
        CancellationToken cancellationToken = default)
    {
        if (await _supplierRepository.ExistsByNameAsync(companyId, name, cancellationToken: cancellationToken))
        {
            return Result<Supplier>.Fail($"Supplier '{name}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(taxId))
        {
            var trimmedTaxId = taxId.Trim();
            if (await _supplierRepository.ExistsByTaxIdAsync(companyId, trimmedTaxId, cancellationToken: cancellationToken))
            {
                return Result<Supplier>.Fail($"Supplier tax id '{trimmedTaxId}' already exists.");
            }
        }

        var supplier = Supplier.Create(companyId, name, taxId, country, currency);
        await _supplierRepository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Supplier>.Ok(supplier);
    }

    public async Task<Result<Warehouse>> CreateWarehouseAsync(
        long companyId,
        string code,
        string name,
        CancellationToken cancellationToken = default,
        string? location = null)
    {
        if (await _warehouseRepository.ExistsByCodeAsync(companyId, code, cancellationToken: cancellationToken))
        {
            return Result<Warehouse>.Fail($"Warehouse code '{code}' already exists.");
        }

        var warehouse = Warehouse.Create(companyId, code, name, location);
        await _warehouseRepository.AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Warehouse>.Ok(warehouse);
    }

    public Task<Warehouse?> GetWarehouseAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _warehouseRepository.GetByIdAsync(companyId, id, cancellationToken);

    public async Task<Result<Warehouse>> UpdateWarehouseAsync(
        long companyId,
        long id,
        string code,
        string name,
        bool isActive,
        CancellationToken cancellationToken = default,
        string? location = null)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (warehouse is null)
        {
            return Result<Warehouse>.Fail("Warehouse not found.");
        }

        if (await _warehouseRepository.ExistsByCodeAsync(companyId, code, warehouse.Id, cancellationToken))
        {
            return Result<Warehouse>.Fail($"Warehouse code '{code}' already exists.");
        }

        warehouse.Update(code, name, isActive, location);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Warehouse>.Ok(warehouse);
    }

    public async Task<Result> DeleteWarehouseAsync(long companyId, long id, CancellationToken cancellationToken = default)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (warehouse is null)
        {
            return Result.Fail("Warehouse not found.");
        }

        _warehouseRepository.Remove(warehouse);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public Task<IReadOnlyList<Customer>> ListCustomersAsync(long companyId, CancellationToken cancellationToken = default)
        => _customerRepository.ListAsync(companyId, cancellationToken);

    public Task<Customer?> GetCustomerAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _customerRepository.GetByIdAsync(companyId, id, cancellationToken);

    public async Task<Result<Customer>> UpdateCustomerAsync(
        long companyId,
        long id,
        string name,
        string? taxId,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (customer is null)
        {
            return Result<Customer>.Fail("Customer not found.");
        }

        if (await _customerRepository.ExistsByNameAsync(companyId, name, customer.Id, cancellationToken))
        {
            return Result<Customer>.Fail($"Customer '{name}' already exists.");
        }

        customer.Update(name, taxId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Customer>.Ok(customer);
    }

    public async Task<Result> DeleteCustomerAsync(long companyId, long id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(companyId, id, cancellationToken);
        if (customer is null)
        {
            return Result.Fail("Customer not found.");
        }

        _customerRepository.Remove(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public Task<IReadOnlyList<Product>> ListProductsAsync(long companyId, CancellationToken cancellationToken = default)
        => _productRepository.ListAsync(companyId, cancellationToken);

    public Task<Product?> GetProductAsync(long companyId, long id, CancellationToken cancellationToken = default)
        => _productRepository.GetByIdAsync(companyId, id, cancellationToken);

    public Task<IReadOnlyList<Supplier>> ListSuppliersAsync(long companyId, CancellationToken cancellationToken = default)
        => _supplierRepository.ListAsync(companyId, isActive: true, cancellationToken: cancellationToken);

    public Task<IReadOnlyList<Warehouse>> ListWarehousesAsync(long companyId, CancellationToken cancellationToken = default)
        => _warehouseRepository.ListAsync(companyId, cancellationToken);
}
