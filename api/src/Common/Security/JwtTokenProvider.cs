namespace SteadyLearn.Common.Security;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using SteadyLearn.Common.Options;
using SteadyLearn.Domain.Entities;

/// <summary>
/// JWT token generation and validation service.
/// </summary>
public interface IJwtTokenProvider
{
    /// <summary>
    /// Generates an access token for the given user.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Generates a secure random token for email verification or password reset.
    /// </summary>
    string GenerateSecureToken();

    /// <summary>
    /// Hashes a token for secure storage.
    /// </summary>
    string HashToken(string token);

    /// <summary>
    /// Validates a token hash against a plain token.
    /// </summary>
    bool ValidateTokenHash(string token, string hash);

    /// <summary>
    /// Gets claims from an expired access token.
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

/// <summary>
/// Implementation of JWT token provider.
/// </summary>
public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly JwtOptions _options;
    private readonly byte[] _secretKey;

    public JwtTokenProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value ?? throw new InvalidOperationException("JwtOptions are not configured");
        if (string.IsNullOrWhiteSpace(_options.SecretKey))
        {
            throw new InvalidOperationException("JWT SecretKey not configured");
        }

        _secretKey = Encoding.UTF8.GetBytes(_options.SecretKey);
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(user.FirstName))
            claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));

        if (!string.IsNullOrEmpty(user.LastName))
            claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

        var key = new SymmetricSecurityKey(_secretKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTimeOffset.UtcNow.UtcDateTime,
            expires: DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenExpirationMinutes).UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public string GenerateSecureToken()
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    public string HashToken(string token)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool ValidateTokenHash(string token, string hash)
    {
        var computedHash = HashToken(token);
        return computedHash == hash;
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false, // We want to get claims from expired tokens
            ValidateIssuerSigningKey = true,
            ValidIssuer = _options.Issuer,
            ValidAudience = _options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(_secretKey)
        };

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}
