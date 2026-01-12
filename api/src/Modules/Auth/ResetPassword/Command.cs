namespace SteadyLearn.Modules.Auth.ResetPassword;

using SteadyLearn.Common.Abstractions.Messaging;

/// <summary>
/// Command to request a password reset.
/// </summary>
public record RequestPasswordResetCommand : ICommand<RequestPasswordResetResponse>
{
    public required string Email { get; init; }
}

/// <summary>
/// Response returned after password reset request.
/// </summary>
public record RequestPasswordResetResponse
{
    public required string Message { get; init; }
}

/// <summary>
/// Command to complete password reset.
/// </summary>
public record CompletePasswordResetCommand : ICommand<CompletePasswordResetResponse>
{
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}

/// <summary>
/// Response returned after successful password reset.
/// </summary>
public record CompletePasswordResetResponse
{
    public required string Message { get; init; }
}
