namespace SteadyLearn.Modules.Auth.Logout;

using SteadyLearn.Common.Abstractions.Messaging;

/// <summary>
/// Command to logout a user (invalidate refresh token).
/// </summary>
public record LogoutCommand : ICommand<LogoutResponse>
{
    public required Guid UserId { get; init; }
}

/// <summary>
/// Response returned after successful logout.
/// </summary>
public record LogoutResponse
{
    public required string Message { get; init; }
}
