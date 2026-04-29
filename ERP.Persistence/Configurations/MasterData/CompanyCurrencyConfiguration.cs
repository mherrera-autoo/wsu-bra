using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.MasterData;

public sealed class CompanyCurrencyConfiguration : IEntityTypeConfiguration<CompanyCurrency>
{
    public void Configure(EntityTypeBuilder<CompanyCurrency> builder)
    {
        builder.ToTable("CompanyCurrencies", "public");
        builder.HasKey(cc => cc.Id);

        builder.Property(cc => cc.CompanyId).IsRequired();
        builder.Property(cc => cc.CurrencyId).IsRequired();
        builder.Property(cc => cc.IsDefault).HasDefaultValue(false);
        builder.Property(cc => cc.IsActive).HasDefaultValue(true);

        // Foreign key relationships
        builder.HasOne(cc => cc.Company)
            .WithMany()
            .HasForeignKey(cc => cc.CompanyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cc => cc.Currency)
            .WithMany()
            .HasForeignKey(cc => cc.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraint for Company-Currency combination
        builder.HasIndex(cc => new { cc.CompanyId, cc.CurrencyId })
            .IsUnique()
            .HasDatabaseName("IX_CompanyCurrencies_CompanyId_CurrencyId");

        // Filtered unique index for default currency per company
        builder.HasIndex(cc => cc.CompanyId)
            .HasFilter("\"IsDefault\" = true")
            .IsUnique()
            .HasDatabaseName("IX_CompanyCurrencies_CompanyId_Default");

        // Indexes for performance
        builder.HasIndex(cc => cc.CompanyId)
            .HasDatabaseName("IX_CompanyCurrencies_CompanyId");

        builder.HasIndex(cc => cc.CurrencyId)
            .HasDatabaseName("IX_CompanyCurrencies_CurrencyId");
    }
}