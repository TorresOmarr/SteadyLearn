namespace SteadyLearn.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SteadyLearn.Domain.Entities;

/// <summary>
/// Entity framework configuration for User entity.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {

        // Primary Key
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        // Unique Constraints
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("\"deleted_at\" IS NULL")
            .HasDatabaseName("IX_Users_Email_Unique");

        // Property Configuration
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");

        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .HasColumnType("varchar(100)");

        builder.Property(u => u.Role)
            .HasConversion<int>();

        builder.Property(u => u.IsEmailVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.EmailVerifiedAt)
            .HasColumnType("timestamp with time zone");


        builder.Property(u => u.PasswordResetTokenHash)
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");

        builder.Property(u => u.PasswordResetTokenExpiresAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.EmailVerificationTokenHash)
            .HasMaxLength(255)
            .HasColumnType("varchar(255)");

        builder.Property(u => u.EmailVerificationTokenExpiresAt)
            .HasColumnType("timestamp with time zone");

        // Audit Fields
        builder.Property(u => u.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.DeletedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);
    }
}
