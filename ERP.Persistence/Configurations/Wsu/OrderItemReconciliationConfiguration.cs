using ERP.Modules.Wsu.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.Wsu;

public sealed class OrderItemReconciliationConfiguration : IEntityTypeConfiguration<OrderItemReconciliation>
{
    private const string Schema = "wsu";

    public void Configure(EntityTypeBuilder<OrderItemReconciliation> builder)
    {
        builder.ToTable("OrderItemReconciliations", Schema, tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_OrderItemReconciliations_ReconciledQuantity", "\"ReconciledQuantity\" > 0");
        });

        builder.HasKey(item => item.Id);
        builder.Property(item => item.PublicId).IsRequired();
        builder.Property(item => item.OrderId).IsRequired();
        builder.Property(item => item.OrderItemId).IsRequired();
        builder.Property(item => item.ReconciledQuantity).HasColumnType("numeric(18,3)").IsRequired();
        builder.Property(item => item.ReconciledAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(item => item.ReconciledByUserPublicId);
        builder.Property(item => item.SourceType).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(item => item.Notes).HasMaxLength(1024);

        builder.HasIndex(item => item.PublicId).IsUnique();
        builder.HasIndex(item => item.OrderId);
        builder.HasIndex(item => item.OrderItemId);

        builder.HasOne(item => item.OrderItem)
            .WithMany()
            .HasForeignKey(item => item.OrderItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
