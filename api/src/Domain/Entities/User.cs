namespace SteadyLearn.Domain.Entities;

using SteadyLearn.Domain.Interfaces;

/// <summary>
/// Represents a user in the SteadyLearn system.
/// </summary>
public class User : IAuditableEntity
{
    // Primary Key
    public Guid Id { get; set; } = Guid.NewGuid();

    // Account Information
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    // Profile Information
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    // Account Status
    public UserRole Role { get; set; } = UserRole.Student;
    public bool IsEmailVerified { get; set; } = false;
    public DateTime? EmailVerifiedAt { get; set; }

    // Authentication Tokens
    public string? RefreshTokenHash { get; set; }
    public DateTime? RefreshTokenExpiresAt { get; set; }
    public string? RefreshTokenFamily { get; set; } // For token rotation tracking

    // Password Reset
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    // Email Verification
    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationTokenExpiresAt { get; set; }

    // Audit Fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string GetFullName()
    {
        var parts = new[] { FirstName, LastName }.Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
        return parts.Count > 0 ? string.Join(" ", parts) : Email;
    }

    /// <summary>
    /// Invalidates the current refresh token (used for logout and token rotation).
    /// </summary>
    public void InvalidateRefreshToken()
    {
        RefreshTokenHash = null;
        RefreshTokenExpiresAt = null;
        RefreshTokenFamily = null;
    }

    /// <summary>
    /// Checks if the refresh token has expired.
    /// </summary>
    public bool IsRefreshTokenExpired()
    {
        if (RefreshTokenExpiresAt == null)
            return true;

        return DateTime.UtcNow > RefreshTokenExpiresAt;
    }

    /// <summary>
    /// Checks if the email verification token has expired.
    /// </summary>
    public bool IsEmailVerificationTokenExpired()
    {
        if (EmailVerificationTokenExpiresAt == null)
            return true;

        return DateTime.UtcNow > EmailVerificationTokenExpiresAt;
    }

    /// <summary>
    /// Checks if the password reset token has expired.
    /// </summary>
    public bool IsPasswordResetTokenExpired()
    {
        if (PasswordResetTokenExpiresAt == null)
            return true;

        return DateTime.UtcNow > PasswordResetTokenExpiresAt;
    }

    /// <summary>
    /// Marks the user's email as verified.
    /// </summary>
    public void MarkEmailAsVerified()
    {
        IsEmailVerified = true;
        EmailVerifiedAt = DateTime.UtcNow;
        EmailVerificationTokenHash = null;
        EmailVerificationTokenExpiresAt = null;
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
        UpdatedAt = DateTime.UtcNow;
    }
}
