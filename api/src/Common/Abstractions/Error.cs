namespace SteadyLearn.Common.Abstractions;

/// <summary>
/// Represents a domain error with a language-agnostic error code.
/// The frontend is responsible for translating error codes to user-friendly messages.
/// </summary>
public sealed record Error
{
    /// <summary>
    /// Language-agnostic error code (e.g., "EMAIL_ALREADY_EXISTS", "INVALID_CREDENTIALS").
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// Optional developer-friendly message for debugging.
    /// Not sent to client in production.
    /// </summary>
    public string? Message { get; init; }

    public Error(string code, string? message = null)
    {
        Code = code;
        Message = message;
    }

    public static Error Create(string code, string? message = null)
        => new(code, message);
}
