using ERP.Modules.Wsu.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.Wsu;

public sealed class WsuInventoryMovementOperatorConfiguration : IEntityTypeConfiguration<WsuInventoryMovementOperator>
{
    private const string Schema = "wsu";

    public void Configure(EntityTypeBuilder<WsuInventoryMovementOperator> builder)
    {
        builder.ToTable("InventoryMovementOperators", Schema);

        builder.HasKey(item => item.Id);
        builder.Property(item => item.PublicId).IsRequired();
        builder.Property(item => item.InventoryMovementId).IsRequired();
        builder.Property(item => item.CodigoOperador).HasMaxLength(64).IsRequired();
        builder.Property(item => item.NombreOperador).HasMaxLength(256);
        builder.Property(item => item.FechaMovimiento).HasColumnType("date");
        builder.Property(item => item.HoraInicioMovimiento).HasColumnType("time");
        builder.Property(item => item.HoraFinMovimiento).HasColumnType("time");

        builder.HasIndex(item => item.PublicId).IsUnique();
        builder.HasIndex(item => item.InventoryMovementId);
        builder.HasIndex(item => new { item.InventoryMovementId, item.CodigoOperador });

        builder.HasOne(item => item.InventoryMovement)
            .WithMany(item => item.Operators)
            .HasForeignKey(item => item.InventoryMovementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
