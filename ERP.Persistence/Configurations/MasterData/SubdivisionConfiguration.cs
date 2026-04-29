using ERP.Modules.MasterData.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.MasterData;

public sealed class SubdivisionConfiguration : IEntityTypeConfiguration<Subdivision>
{
    public void Configure(EntityTypeBuilder<Subdivision> builder)
    {
        builder.ToTable("Subdivision", "masterdata");
        builder.HasKey(subdivision => subdivision.Id);
        builder.Property(subdivision => subdivision.CountryId).IsRequired();
        builder.Property(subdivision => subdivision.Code).HasMaxLength(12).IsRequired();
        builder.Property(subdivision => subdivision.Name).HasMaxLength(150).IsRequired();
        builder.Property(subdivision => subdivision.Level).IsRequired();
        builder.Property(subdivision => subdivision.IsActive).HasDefaultValue(true);
        builder.HasOne(subdivision => subdivision.Country)
            .WithMany()
            .HasForeignKey(subdivision => subdivision.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(subdivision => subdivision.ParentSubdivision)
            .WithMany()
            .HasForeignKey(subdivision => subdivision.ParentSubdivisionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(subdivision => new { subdivision.CountryId, subdivision.Code }).IsUnique();
        builder.HasIndex(subdivision => new { subdivision.CountryId, subdivision.Level });
        builder.HasIndex(subdivision => subdivision.ParentSubdivisionId);
        builder.HasIndex(subdivision => subdivision.Name);
    }
}
