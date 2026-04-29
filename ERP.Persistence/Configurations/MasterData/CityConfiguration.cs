using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.MasterData;

public sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("City", "masterdata");
        builder.HasKey(city => city.Id);
        builder.Property(city => city.CountryId).IsRequired();
        builder.Property(city => city.Name).HasMaxLength(150).IsRequired();
        builder.Property(city => city.OfficialCode).HasMaxLength(16);
        builder.Property(city => city.PostalCode).HasMaxLength(20);
        builder.Property(city => city.IsCapital).HasDefaultValue(false);
        builder.Property(city => city.IsActive).HasDefaultValue(true);
        builder.HasOne(city => city.Country)
            .WithMany()
            .HasForeignKey(city => city.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(city => city.Subdivision)
            .WithMany()
            .HasForeignKey(city => city.SubdivisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(city => city.CountryId);
        builder.HasIndex(city => city.SubdivisionId);
        builder.HasIndex(city => city.OfficialCode);
        builder.HasIndex(city => new { city.CountryId, city.OfficialCode })
            .IsUnique()
            .HasFilter("\"OfficialCode\" IS NOT NULL");
        builder.HasIndex(city => new { city.CountryId, city.SubdivisionId, city.Name }).IsUnique();
        builder.HasIndex(city => new { city.CountryId, city.Name })
            .IsUnique()
            .HasFilter("\"SubdivisionId\" IS NULL");
    }
}
