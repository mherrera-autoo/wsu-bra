using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.MasterData;

public sealed class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currency", "masterdata");
        builder.HasKey(currency => currency.Id);
        
        builder.Property(currency => currency.Code).HasMaxLength(3).IsRequired();
        builder.Property(currency => currency.NumericCode).IsRequired();
        builder.Property(currency => currency.Name).HasMaxLength(200).IsRequired();
        builder.Property(currency => currency.Symbol).HasMaxLength(10);
        builder.Property(currency => currency.MinorUnits).IsRequired();
        builder.Property(currency => currency.Order).IsRequired();
        builder.Property(currency => currency.IsActive).HasDefaultValue(true);
        
        builder.HasIndex(currency => currency.Code).IsUnique();
        builder.HasIndex(currency => currency.NumericCode).IsUnique();
        builder.HasIndex(currency => currency.Code);
    }
}