using System.ComponentModel.DataAnnotations;

namespace SteadyLearn.Common.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Range(1, 168)]
    public int VerificationTokenExpirationHours { get; init; } = 24;

    [Range(1, 168)]
    public int ResetTokenExpirationHours { get; init; } = 24;

    public bool MockEmailOutput { get; init; } = true;

    [Required, Url]
    public string BaseUrl { get; init; } = "http://localhost:5000";

    [Required, Url]
    public string ResetBaseUrl { get; init; } = "http://localhost:3000";
}
