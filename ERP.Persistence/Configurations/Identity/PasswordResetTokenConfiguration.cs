using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.Identity;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens", "iam");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.Property(x => x.UsedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiresAtUtc);
        builder.HasIndex(x => x.UsedAtUtc);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.Property<uint>("xmin")
            .IsRowVersion()
            .HasColumnName("xmin");
    }
}