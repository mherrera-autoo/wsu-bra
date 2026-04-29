using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.MasterData;

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Address", "masterdata");
        builder.HasKey(address => address.Id);
        builder.Property(address => address.CountryId).IsRequired();
        builder.Property(address => address.Street).HasMaxLength(180).IsRequired();
        builder.Property(address => address.Number).HasMaxLength(30);
        builder.Property(address => address.Unit).HasMaxLength(30);
        builder.Property(address => address.PostalCode).HasMaxLength(20);
        builder.Property(address => address.Notes).HasMaxLength(250);
        builder.Property(address => address.GeoLat).HasPrecision(9, 6);
        builder.Property(address => address.GeoLng).HasPrecision(9, 6);
        builder.HasOne(address => address.Country)
            .WithMany()
            .HasForeignKey(address => address.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(address => address.Level1Subdivision)
            .WithMany()
            .HasForeignKey(address => address.Level1SubdivisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(address => address.Level2Subdivision)
            .WithMany()
            .HasForeignKey(address => address.Level2SubdivisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(address => address.City)
            .WithMany()
            .HasForeignKey(address => address.CityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(address => address.Locality)
            .WithMany()
            .HasForeignKey(address => address.LocalityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(address => address.CountryId);
        builder.HasIndex(address => address.Level1SubdivisionId);
        builder.HasIndex(address => address.Level2SubdivisionId);
        builder.HasIndex(address => address.CityId);
        builder.HasIndex(address => address.LocalityId);
    }
}
