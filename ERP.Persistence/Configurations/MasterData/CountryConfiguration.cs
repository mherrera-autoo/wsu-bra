using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.MasterData;

public sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Country", "masterdata");
        builder.HasKey(country => country.Id);
        builder.Property(country => country.Iso2).HasMaxLength(2).IsRequired();
        builder.Property(country => country.Iso3).HasMaxLength(3).IsRequired();
        builder.Property(country => country.Name).HasMaxLength(120).IsRequired();
        builder.Property(country => country.NumericCode).HasMaxLength(3);
        builder.Property(country => country.PhonePrefix).HasMaxLength(8);
        builder.Property(country => country.CurrencyCode).HasMaxLength(3);
        builder.Property(country => country.IsActive).HasDefaultValue(true);
        builder.HasIndex(country => country.Iso2).IsUnique();
        builder.HasIndex(country => country.Iso3).IsUnique();
        builder.HasIndex(country => country.Name);
    }
}
