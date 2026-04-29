using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.MasterData;

public sealed class ProductSupplierConfiguration : IEntityTypeConfiguration<ProductSupplier>
{
    public void Configure(EntityTypeBuilder<ProductSupplier> builder)
    {
        builder.ToTable("ProductSuppliers", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_ProductSuppliers_UnitPurchasePrice", "\"UnitPurchasePrice\" >= 0");
            tableBuilder.HasCheckConstraint("CK_ProductSuppliers_MinimumPurchaseLot", "\"MinimumPurchaseLot\" > 0");
        });

        builder.HasKey(ps => ps.Id);

        builder.Property(ps => ps.SupplierSku)
            .HasMaxLength(128);

        builder.Property(ps => ps.UnitPurchasePrice)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(ps => ps.MinimumPurchaseLot)
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.HasOne(ps => ps.Product)
            .WithMany(p => p.ProductSuppliers)
            .HasForeignKey(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ps => ps.Supplier)
            .WithMany(s => s.ProductSuppliers)
            .HasForeignKey(ps => ps.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Company>()
            .WithMany()
            .HasForeignKey(ps => ps.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ps => new { ps.ProductId, ps.SupplierId })
            .IsUnique()
            .HasDatabaseName("IX_ProductSuppliers_ProductId_SupplierId");

        builder.HasIndex(ps => ps.ProductId)
            .HasDatabaseName("IX_ProductSuppliers_ProductId");

        builder.HasIndex(ps => ps.SupplierId)
            .HasDatabaseName("IX_ProductSuppliers_SupplierId");
    }
}
