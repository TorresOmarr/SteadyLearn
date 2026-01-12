namespace SteadyLearn.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteadyLearn.Domain.Entities;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {

        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id).ValueGeneratedNever();

        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");

        builder.Property(rt => rt.Family)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(rt => rt.Status)
            .HasConversion<int>();

        builder.Property(rt => rt.IssuedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(rt => rt.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(rt => rt.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(rt => new { rt.UserId, rt.Status });
        builder.HasIndex(rt => new { rt.UserId, rt.Family });
        builder.HasIndex(rt => new { rt.UserId, rt.ExpiresAt });
    }
}
