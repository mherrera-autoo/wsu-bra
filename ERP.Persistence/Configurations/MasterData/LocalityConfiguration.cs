using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.MasterData;

public sealed class LocalityConfiguration : IEntityTypeConfiguration<Locality>
{
    public void Configure(EntityTypeBuilder<Locality> builder)
    {
        builder.ToTable("Locality", "masterdata");
        builder.HasKey(locality => locality.Id);
        builder.Property(locality => locality.CityId).IsRequired();
        builder.Property(locality => locality.Name).HasMaxLength(150).IsRequired();
        builder.Property(locality => locality.IsActive).HasDefaultValue(true);
        builder.HasOne(locality => locality.City)
            .WithMany()
            .HasForeignKey(locality => locality.CityId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(locality => new { locality.CityId, locality.Name }).IsUnique();
        builder.HasIndex(locality => locality.CityId);
    }
}
