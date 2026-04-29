using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ERP.Persistence.Persistence;

public sealed class ErpDbContextFactory : IDesignTimeDbContextFactory<ErpDbContext>
{
    public ErpDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ErpDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ERP_DB")
            ?? "Host=localhost;Port=5432;Database=erp;Username=erp;Password=erp";

        optionsBuilder.UseNpgsql(connectionString);
        return new ErpDbContext(optionsBuilder.Options);
    }
}
