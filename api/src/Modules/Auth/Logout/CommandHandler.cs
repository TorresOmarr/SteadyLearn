namespace SteadyLearn.Modules.Auth.Logout;

using Microsoft.EntityFrameworkCore;
using SteadyLearn.Common.Abstractions;
using SteadyLearn.Common.Abstractions.Messaging;
using SteadyLearn.Common.Constants;
using SteadyLearn.Common.Security;
using SteadyLearn.Data;

/// <summary>
/// Handler for user logout.
/// </summary>
public class LogoutCommandHandler : ICommandHandler<LogoutCommand, LogoutResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IRefreshTokenService _refreshTokenService;

    public LogoutCommandHandler(ApplicationDbContext context, IRefreshTokenService refreshTokenService)
    {
        _context = context;
        _refreshTokenService = refreshTokenService;
    }

    public async Task<Result<LogoutResponse>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return Result.Failure<LogoutResponse>(
                ErrorCodes.UserNotFound,
                "User not found");
        }

        // Revoke all refresh token families for this user
        var families = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id)
            .Select(rt => rt.Family)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var family in families)
        {
            await _refreshTokenService.RevokeFamilyAsync(user.Id, family, cancellationToken);
        }

        user.TouchUpdated();

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new LogoutResponse
        {
            Message = "Logged out successfully"
        });
    }
}
