namespace SteadyLearn.Common.Security;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SteadyLearn.Common.Options;

/// <summary>
/// Email service for sending verification and password reset emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email verification link to the user.
    /// </summary>
    Task SendEmailVerificationAsync(string email, string verificationToken);

    /// <summary>
    /// Sends a password reset link to the user.
    /// </summary>
    Task SendPasswordResetAsync(string email, string resetToken);
}

/// <summary>
/// Mock email service that logs emails to console (for development).
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;
    private readonly EmailOptions _options;

    public MockEmailService(ILogger<MockEmailService> logger, IOptions<EmailOptions> options)
    {
        _logger = logger;
        _options = options.Value ?? throw new InvalidOperationException("EmailOptions are not configured");
    }

    public Task SendEmailVerificationAsync(string email, string verificationToken)
    {
        var baseUrl = _options.BaseUrl;
        var verificationLink = $"{baseUrl}/api/auth/verify-email?token={verificationToken}";

        _logger.LogInformation(
            "=== MOCK EMAIL SERVICE ===\n" +
            "To: {Email}\n" +
            "Subject: Verify your SteadyLearn account\n" +
            "Body:\n" +
            "Please click the following link to verify your email:\n" +
            "{VerificationLink}\n" +
            "This link expires in 24 hours.\n" +
            "==========================",
            email, verificationLink);

        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string resetToken)
    {
        var baseUrl = _options.ResetBaseUrl;
        var resetLink = $"{baseUrl}/reset-password?token={resetToken}";

        _logger.LogInformation(
            "=== MOCK EMAIL SERVICE ===\n" +
            "To: {Email}\n" +
            "Subject: Reset your SteadyLearn password\n" +
            "Body:\n" +
            "Click the following link to reset your password:\n" +
            "{ResetLink}\n" +
            "This link expires in 24 hours.\n" +
            "If you didn't request this, please ignore this email.\n" +
            "==========================",
            email, resetLink);

        return Task.CompletedTask;
    }
}
