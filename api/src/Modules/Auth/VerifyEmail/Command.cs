namespace SteadyLearn.Modules.Auth.VerifyEmail;

using SteadyLearn.Common.Abstractions.Messaging;

/// <summary>
/// Command to verify a user's email.
/// </summary>
public record VerifyEmailCommand : ICommand<VerifyEmailResponse>
{
    public required string Token { get; init; }
}

/// <summary>
/// Response returned after successful email verification.
/// </summary>
public record VerifyEmailResponse
{
    public required string Message { get; init; }
}
