using ERP.Modules.Wsu.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.Wsu;

public sealed class OrderItemConsumptionConfiguration : IEntityTypeConfiguration<OrderItemConsumption>
{
    private const string Schema = "wsu";

    public void Configure(EntityTypeBuilder<OrderItemConsumption> builder)
    {
        builder.ToTable("OrderItemConsumptions", Schema, tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_OrderItemConsumptions_Quantity", "\"Quantity\" > 0");
            tableBuilder.HasCheckConstraint("CK_OrderItemConsumptions_UnitCost", "\"UnitCost\" >= 0");
            tableBuilder.HasCheckConstraint("CK_OrderItemConsumptions_TotalCost", "\"TotalCost\" >= 0");
        });

        builder.HasKey(item => item.Id);

        builder.Property(item => item.PublicId).IsRequired();
        builder.Property(item => item.OutOrderItemId).IsRequired();
        builder.Property(item => item.InOrderItemId).IsRequired();
        builder.Property(item => item.Quantity).HasColumnType("numeric(18,3)").IsRequired();
        builder.Property(item => item.UnitCost).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(item => item.TotalCost).HasColumnType("numeric(18,2)").IsRequired();

        builder.HasIndex(item => item.PublicId).IsUnique();
        builder.HasIndex(item => item.OutOrderItemId);
        builder.HasIndex(item => item.InOrderItemId);
        builder.HasIndex(item => new { item.OutOrderItemId, item.InOrderItemId });
    }
}
