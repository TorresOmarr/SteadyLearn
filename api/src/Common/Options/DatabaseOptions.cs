using System.ComponentModel.DataAnnotations;

namespace SteadyLearn.Common.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required]
    public string DefaultConnection { get; init; } = string.Empty;
}
