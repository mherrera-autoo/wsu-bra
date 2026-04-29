using ERP.Modules.Wsu.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.Wsu;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    private const string Schema = "wsu";

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", Schema);
        builder.HasKey(item => item.Id);

        builder.Property(item => item.PublicId).IsRequired();
        builder.Property(item => item.CompanyPublicId).IsRequired();
        builder.Property(item => item.WarehousePublicId);
        builder.Property(item => item.OrderType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(item => item.OrderNumber).HasMaxLength(64).IsRequired();
        builder.Property(item => item.ExternalOrderNumber).HasMaxLength(64);
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(item => item.MovementDate).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(item => item.CreatedByUserPublicId);
        builder.Property(item => item.SourceType).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(item => item.Notes).HasMaxLength(1024);
        builder.Property(item => item.IsEpcLoadConfirmed).IsRequired();
        builder.Property(item => item.IsEpcSkuMatchCompleted).IsRequired();
        builder.Property(item => item.IsMovementProgrammed).IsRequired().HasDefaultValue(false);

        builder.HasIndex(item => item.PublicId).IsUnique();
        builder.HasIndex(item => new { item.CompanyPublicId, item.OrderNumber }).IsUnique();
        builder.HasIndex(item => new { item.CompanyPublicId, item.MovementDate });
        builder.HasIndex(item => new { item.CompanyPublicId, item.OrderType, item.Status });

        builder.HasMany(item => item.Items)
            .WithOne(item => item.Order)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
