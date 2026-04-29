using ERP.Modules.Wsu.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.Wsu;

public sealed class OrderItemEpcAssignmentConfiguration : IEntityTypeConfiguration<OrderItemEpcAssignment>
{
    private const string Schema = "wsu";

    public void Configure(EntityTypeBuilder<OrderItemEpcAssignment> builder)
    {
        builder.ToTable("OrderItemEpcAssignments", Schema);

        builder.HasKey(item => item.Id);
        builder.Property(item => item.PublicId).IsRequired();
        builder.Property(item => item.OrderId).IsRequired();
        builder.Property(item => item.OrderItemId).IsRequired();
        builder.Property(item => item.Epc).HasMaxLength(128).IsRequired();
        builder.Property(item => item.IsChecked).IsRequired();

        builder.HasIndex(item => item.PublicId).IsUnique();
        builder.HasIndex(item => item.OrderId);
        builder.HasIndex(item => item.OrderItemId);
        builder.HasIndex(item => new { item.OrderId, item.Epc }).IsUnique();

        builder.HasOne(item => item.OrderItem)
            .WithMany()
            .HasForeignKey(item => item.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
