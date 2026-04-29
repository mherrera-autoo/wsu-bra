using ERP.Modules.MasterData.Application.Commands;
using ERP.Modules.MasterData.Application.Handlers;
using ERP.Modules.MasterData.Application.Queries;
using Xunit;

namespace ERP.Modules.MasterData.WhiteBox.Tests;

public sealed class SupplierHandlersTests
{
    [Fact]
    public async Task SupplierFlow_ShouldRespectTenantAndSoftDelete()
    {
        var repository = new InMemorySupplierRepository();
        var unitOfWork = new InMemoryUnitOfWork();
        var commandHandler = new SupplierCommandHandler(repository, unitOfWork);
        var queryHandler = new SupplierQueryHandler(repository);

        var created = await commandHandler.HandleAsync(new CreateSupplierCommand(1, "Supplier A", "123", "CL", "CLP"));

        Assert.True(created.Success);
        var supplierId = created.Value!.Id;

        var fromOtherCompany = await queryHandler.HandleAsync(new GetSupplierByIdQuery(2, supplierId));
        Assert.Null(fromOtherCompany);

        var fetched = await queryHandler.HandleAsync(new GetSupplierByIdQuery(1, supplierId));
        Assert.NotNull(fetched);
        Assert.Equal("Supplier A", fetched!.Name);

        var updated = await commandHandler.HandleAsync(new UpdateSupplierCommand(1, supplierId, "Supplier B", "123", "CL", "USD", true));
        Assert.True(updated.Success);
        Assert.Equal("Supplier B", updated.Value!.Name);

        var list = await queryHandler.HandleAsync(new ListSuppliersQuery(1, null, true));
        Assert.Single(list);

        var deleted = await commandHandler.HandleAsync(new DeleteSupplierCommand(1, supplierId));
        Assert.True(deleted.Success);

        var listAfterDelete = await queryHandler.HandleAsync(new ListSuppliersQuery(1, null, true));
        Assert.Empty(listAfterDelete);

        var updateFromOtherCompany = await commandHandler.HandleAsync(
            new UpdateSupplierCommand(2, supplierId, "Supplier C", "123", "CL", "USD", true));
        Assert.False(updateFromOtherCompany.Success);
        Assert.Equal("Supplier not found.", updateFromOtherCompany.Error);
    }
}
