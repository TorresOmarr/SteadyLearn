namespace SteadyLearn.Common.Abstractions;

/// <summary>
/// Represents a domain error with a language-agnostic error code.
/// The frontend is responsible for translating error codes to user-friendly messages.
/// </summary>
public sealed class Error
{
    /// <summary>
    /// Language-agnostic error code (e.g., "EMAIL_ALREADY_EXISTS", "INVALID_CREDENTIALS").
    /// </summary>
    public string Code { get; init; }

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

    public override bool Equals(object? obj) =>
        obj is Error error && 
        error.Code == Code && 
        error.Message == Message;

    public override int GetHashCode() =>
        HashCode.Combine(Code, Message);

    public override string ToString() =>
        $"Error: {Code}{(Message != null ? $" - {Message}" : string.Empty)}";
}
