namespace SteadyLearn.Modules.Auth.VerifyEmail;

using Microsoft.EntityFrameworkCore;
using SteadyLearn.Common.Abstractions;
using SteadyLearn.Common.Abstractions.Messaging;
using SteadyLearn.Common.Constants;
using SteadyLearn.Common.Security;
using SteadyLearn.Data;

/// <summary>
/// Handler for email verification.
/// </summary>
public class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand, VerifyEmailResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    public VerifyEmailCommandHandler(
        ApplicationDbContext context,
        IJwtTokenProvider jwtTokenProvider)
    {
        _context = context;
        _jwtTokenProvider = jwtTokenProvider;
    }

    public async Task<Result<VerifyEmailResponse>> Handle(
        VerifyEmailCommand request,
        CancellationToken cancellationToken)
    {
        // Hash the token to compare with stored hash
        var tokenHash = _jwtTokenProvider.HashToken(request.Token);

        // Find user by token hash
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.EmailVerificationTokenHash == tokenHash, cancellationToken);

        if (user == null)
        {
            return Result.Failure<VerifyEmailResponse>(
                ErrorCodes.InvalidVerificationToken,
                "Invalid verification token");
        }

        // Check if already verified
        if (user.IsEmailVerified)
        {
            return Result.Failure<VerifyEmailResponse>(
                ErrorCodes.EmailAlreadyVerified,
                "Email is already verified");
        }

        // Check if token is expired
        if (user.IsEmailVerificationTokenExpired())
        {
            return Result.Failure<VerifyEmailResponse>(
                ErrorCodes.VerificationTokenExpired,
                "Verification token has expired. Please request a new one.");
        }

        // Mark email as verified
        user.MarkEmailAsVerified();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new VerifyEmailResponse
        {
            Message = "Email verified successfully. You can now login."
        });
    }
}
