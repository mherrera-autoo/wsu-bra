using ERP.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Persistence.Configurations.Identity;

public sealed class UserPasswordHistoryConfiguration : IEntityTypeConfiguration<UserPasswordHistory>
{
    public void Configure(EntityTypeBuilder<UserPasswordHistory> builder)
    {
        builder.ToTable("UserPasswordHistory", "iam");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .UseIdentityAlwaysColumn();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.PasswordSalt)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired()
            .HasDefaultValueSql("now()");

        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc })
            .IsDescending();

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("now()");

        builder.Property<uint>("xmin")
            .IsRowVersion()
            .HasColumnName("xmin");
    }
}