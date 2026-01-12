namespace SteadyLearn.Modules.Auth.RefreshToken;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SteadyLearn.Common.Abstractions;
using SteadyLearn.Common.Abstractions.Messaging;
using SteadyLearn.Common.Constants;
using SteadyLearn.Common.Options;
using SteadyLearn.Common.Security;
using SteadyLearn.Data;

/// <summary>
/// Handler for token refresh with rotation.
/// </summary>
public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenCommandHandler(
        ApplicationDbContext context,
        IJwtTokenProvider jwtTokenProvider,
        IRefreshTokenService refreshTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _jwtTokenProvider = jwtTokenProvider;
        _refreshTokenService = refreshTokenService;
        _jwtOptions = jwtOptions.Value ?? throw new InvalidOperationException("JwtOptions are not configured");
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Rotate token via service (handles reuse detection and cleanup)
        var (newRefreshToken, createdToken) = await _refreshTokenService.RotateAsync(
            request.RefreshToken,
            cancellationToken);

        var user = await _context.Users.FirstAsync(u => u.Id == createdToken.UserId, cancellationToken);

        // Generate new access token
        var newAccessToken = _jwtTokenProvider.GenerateAccessToken(user);

        return Result.Success(new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = _jwtOptions.AccessTokenExpirationMinutes * 60
        });
    }
}
