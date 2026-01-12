namespace SteadyLearn.Modules.Auth.ResetPassword;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SteadyLearn.Common.Abstractions;
using SteadyLearn.Common.Abstractions.Messaging;
using SteadyLearn.Common.Constants;
using SteadyLearn.Common.Options;
using SteadyLearn.Common.Security;
using SteadyLearn.Data;

/// <summary>
/// Handler for requesting a password reset.
/// </summary>
public class RequestPasswordResetCommandHandler : ICommandHandler<RequestPasswordResetCommand, RequestPasswordResetResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IEmailService _emailService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly EmailOptions _emailOptions;
    private readonly JwtOptions _jwtOptions;

    public RequestPasswordResetCommandHandler(
        ApplicationDbContext context,
        IJwtTokenProvider jwtTokenProvider,
        IEmailService emailService,
        IRefreshTokenService refreshTokenService,
        IOptions<EmailOptions> emailOptions,
        IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _jwtTokenProvider = jwtTokenProvider;
        _emailService = emailService;
        _refreshTokenService = refreshTokenService;
        _emailOptions = emailOptions.Value ?? throw new InvalidOperationException("EmailOptions are not configured");
        _jwtOptions = jwtOptions.Value ?? throw new InvalidOperationException("JwtOptions are not configured");
    }

    public async Task<Result<RequestPasswordResetResponse>> Handle(
        RequestPasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        // Always return success to prevent email enumeration attacks
        if (user == null)
        {
            return Result.Success(new RequestPasswordResetResponse
            {
                Message = "If an account with this email exists, you will receive a password reset link."
            });
        }

        // Generate reset token
        var resetToken = _jwtTokenProvider.GenerateSecureToken();
        var resetTokenHash = _jwtTokenProvider.HashToken(resetToken);
        var resetTokenExpiry = _emailOptions.ResetTokenExpirationHours;

        // Update user with reset token
        user.SetPasswordResetToken(resetTokenHash, DateTimeOffset.UtcNow.AddHours(resetTokenExpiry));

        await _context.SaveChangesAsync(cancellationToken);

        // Send password reset email (async, fire-and-forget)
        _ = _emailService.SendPasswordResetAsync(user.Email, resetToken);

        return Result.Success(new RequestPasswordResetResponse
        {
            Message = "If an account with this email exists, you will receive a password reset link."
        });
    }
}

/// <summary>
/// Handler for completing a password reset.
/// </summary>
public class CompletePasswordResetCommandHandler : ICommandHandler<CompletePasswordResetCommand, CompletePasswordResetResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenService _refreshTokenService;

    public CompletePasswordResetCommandHandler(
        ApplicationDbContext context,
        IJwtTokenProvider jwtTokenProvider,
        IPasswordHasher passwordHasher,
        IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _jwtTokenProvider = jwtTokenProvider;
        _passwordHasher = passwordHasher;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<Result<CompletePasswordResetResponse>> Handle(
        CompletePasswordResetCommand request,
        CancellationToken cancellationToken)
    {
        // Hash the token to compare with stored hash
        var tokenHash = _jwtTokenProvider.HashToken(request.Token);

        // Find user by reset token hash
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetTokenHash == tokenHash, cancellationToken);

        if (user == null)
        {
            return Result.Failure<CompletePasswordResetResponse>(
                ErrorCodes.InvalidToken,
                "Invalid or expired reset token");
        }

        // Check if token is expired
        if (user.IsPasswordResetTokenExpired())
        {
            return Result.Failure<CompletePasswordResetResponse>(
                ErrorCodes.TokenExpired,
                "Password reset token has expired. Please request a new one.");
        }

        // Update password
        var passwordHash = _passwordHasher.Hash(request.NewPassword);
        user.SetNewPassword(passwordHash);

        // Invalidate all refresh tokens for security
        var families = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .Select(rt => rt.Family)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var family in families)
        {
            await _refreshTokenService.RevokeFamilyAsync(user.Id, family, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new CompletePasswordResetResponse
        {
            Message = "Password reset successfully. You can now login with your new password."
        });
    }
}
