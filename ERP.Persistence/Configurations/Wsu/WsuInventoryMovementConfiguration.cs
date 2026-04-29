using ERP.Modules.Wsu.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.Wsu;

public sealed class WsuInventoryMovementConfiguration : IEntityTypeConfiguration<WsuInventoryMovement>
{
    private const string Schema = "wsu";

    public void Configure(EntityTypeBuilder<WsuInventoryMovement> builder)
    {
        builder.ToTable("InventoryMovements", Schema, tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_WsuInventoryMovements_Quantity", "\"Quantity\" > 0");
            tableBuilder.HasCheckConstraint("CK_WsuInventoryMovements_SignedQuantity", "\"SignedQuantity\" <> 0");
        });

        builder.HasKey(item => item.Id);

        builder.Property(item => item.PublicId).IsRequired();
        builder.Property(item => item.CompanyPublicId).IsRequired();
        builder.Property(item => item.WarehousePublicId);
        builder.Property(item => item.OrderId).IsRequired();
        builder.Property(item => item.OrderItemId).IsRequired();
        builder.Property(item => item.ProductPublicId);
        builder.Property(item => item.MovementType).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(item => item.Quantity).HasColumnType("numeric(18,3)").IsRequired();
        builder.Property(item => item.SignedQuantity).HasColumnType("numeric(18,3)").IsRequired();
        builder.Property(item => item.OccurredAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(item => item.PerformedByUserPublicId);
        builder.Property(item => item.SourceType).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(item => item.Notes).HasMaxLength(1024);

        builder.HasIndex(item => item.PublicId).IsUnique();
        builder.HasIndex(item => item.OrderId);
        builder.HasIndex(item => item.OrderItemId);
        builder.HasIndex(item => new { item.CompanyPublicId, item.OccurredAt });
        builder.HasIndex(item => new { item.CompanyPublicId, item.ProductPublicId, item.OccurredAt });

        builder.HasOne(item => item.Order)
            .WithMany()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
