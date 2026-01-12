namespace SteadyLearn.Domain.Entities;

using SteadyLearn.Domain.Interfaces;

/// <summary>
/// Represents a user in the SteadyLearn system.
/// </summary>
public class User : IAuditableEntity
{
    // Primary Key
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Account Information
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;

    // Profile Information
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }

    // Account Status
    public UserRole Role { get; private set; } = UserRole.Student;
    public bool IsEmailVerified { get; private set; } = false;
    public DateTimeOffset? EmailVerifiedAt { get; private set; }

    // Password Reset
    public string? PasswordResetTokenHash { get; private set; }
    public DateTimeOffset? PasswordResetTokenExpiresAt { get; private set; }

    // Email Verification
    public string? EmailVerificationTokenHash { get; private set; }
    public DateTimeOffset? EmailVerificationTokenExpiresAt { get; private set; }
    // Audit Fields
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public bool IsDeleted { get; private set; } = false;


    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string GetFullName()
    {
        var parts = new[] { FirstName, LastName }.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        return parts.Count > 0 ? string.Join(" ", parts) : Email;
    }

    public static User Create(string email, string passwordHash, string? firstName = null, string? lastName = null, UserRole role = UserRole.Student)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLower().Trim(),
            PasswordHash = passwordHash,
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim(),
            Role = role,
            CreatedAt = DateTimeOffset.UtcNow,
            IsEmailVerified = false,
            IsDeleted = false
        };

        return user;
    }

    /// <summary>
    /// Invalidates the current refresh token (used for logout and token rotation).
    /// </summary>


    /// <summary>
    /// Checks if the email verification token has expired.
    /// </summary>
    public bool IsEmailVerificationTokenExpired()
    {
        if (EmailVerificationTokenExpiresAt == null)
            return true;

        return DateTimeOffset.UtcNow > EmailVerificationTokenExpiresAt;
    }

    /// <summary>
    /// Checks if the password reset token has expired.
    /// </summary>
    public bool IsPasswordResetTokenExpired()
    {
        if (PasswordResetTokenExpiresAt == null)
            return true;

        return DateTimeOffset.UtcNow > PasswordResetTokenExpiresAt;
    }

    /// <summary>
    /// Marks the user's email as verified.
    /// </summary>
    public void MarkEmailAsVerified()
    {
        IsEmailVerified = true;
        EmailVerifiedAt = DateTimeOffset.UtcNow;
        EmailVerificationTokenHash = null;
        EmailVerificationTokenExpiresAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the user's email as not verified (for resending verification).
    /// </summary>
    public void MarkEmailAsNotVerified()
    {
        IsEmailVerified = false;
        EmailVerifiedAt = null;
    }

    /// <summary>
    /// Sets a new password hash (used after password reset).
    /// </summary>
    public void SetNewPassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        PasswordResetTokenHash = null;
        PasswordResetTokenExpiresAt = null;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetProfile(string? firstName, string? lastName)
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetRole(UserRole role)
    {
        Role = role;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetEmailVerificationToken(string tokenHash, DateTimeOffset expiresAt)
    {
        EmailVerificationTokenHash = tokenHash;
        EmailVerificationTokenExpiresAt = expiresAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPasswordResetToken(string tokenHash, DateTimeOffset expiresAt)
    {
        PasswordResetTokenHash = tokenHash;
        PasswordResetTokenExpiresAt = expiresAt;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DeletedAt;
    }

    public void TouchUpdated()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
