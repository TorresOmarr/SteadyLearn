using System.ComponentModel.DataAnnotations;

namespace SteadyLearn.Common.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32)]
    public string SecretKey { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = "SteadyLearn.API";

    [Required]
    public string Audience { get; init; } = "SteadyLearn.Client";

    [Range(1, 1440)]
    public int AccessTokenExpirationMinutes { get; init; } = 15;

    [Range(1, 30)]
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
