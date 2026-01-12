namespace SteadyLearn.Modules.Auth.Login;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SteadyLearn.Common.Abstractions;
using SteadyLearn.Common.Abstractions.Messaging;
using SteadyLearn.Common.Constants;
using SteadyLearn.Common.Options;
using SteadyLearn.Common.Security;
using SteadyLearn.Data;

/// <summary>
/// Handler for user login.
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly JwtOptions _jwtOptions;

    public LoginCommandHandler(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenProvider jwtTokenProvider,
        IRefreshTokenService refreshTokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenProvider = jwtTokenProvider;
        _refreshTokenService = refreshTokenService;
        _jwtOptions = jwtOptions.Value ?? throw new InvalidOperationException("JwtOptions are not configured");
    }

    public async Task<Result<LoginResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (user == null)
        {
            return Result.Failure<LoginResponse>(
                ErrorCodes.InvalidCredentials,
                "Invalid email or password");
        }

        // Verify password
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<LoginResponse>(
                ErrorCodes.InvalidCredentials,
                "Invalid email or password");
        }

        // Check if email is verified
        if (!user.IsEmailVerified)
        {
            return Result.Failure<LoginResponse>(
                ErrorCodes.AccountNotVerified,
                "Please verify your email before logging in");
        }

        // Generate tokens
        var accessToken = _jwtTokenProvider.GenerateAccessToken(user);
        var (refreshToken,_) = await _refreshTokenService.CreateAsync(user, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtOptions.AccessTokenExpirationMinutes * 60, // Convert to seconds
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role.ToString()
            }
        });
    }
}
