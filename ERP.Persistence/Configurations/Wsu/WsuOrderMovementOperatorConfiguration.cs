using ERP.Modules.Wsu.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.Wsu;

public sealed class WsuOrderMovementOperatorConfiguration : IEntityTypeConfiguration<WsuOrderMovementOperator>
{
    private const string Schema = "wsu";

    public void Configure(EntityTypeBuilder<WsuOrderMovementOperator> builder)
    {
        builder.ToTable("OrderMovementOperators", Schema);

        builder.HasKey(item => item.Id);
        builder.Property(item => item.PublicId).IsRequired();
        builder.Property(item => item.OrderId).IsRequired();
        builder.Property(item => item.CodigoOperador).HasMaxLength(64).IsRequired();
        builder.Property(item => item.NombreOperador).HasMaxLength(256);
        builder.Property(item => item.FechaMovimiento).HasColumnType("date");
        builder.Property(item => item.HoraInicioMovimiento).HasColumnType("time");
        builder.Property(item => item.HoraFinMovimiento).HasColumnType("time");

        builder.HasIndex(item => item.PublicId).IsUnique();
        builder.HasIndex(item => item.OrderId);
        builder.HasIndex(item => new { item.OrderId, item.CodigoOperador });

        builder.HasOne(item => item.Order)
            .WithMany(item => item.MovementOperators)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
