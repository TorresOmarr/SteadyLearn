namespace SteadyLearn.Modules.Auth.RefreshToken;

using SteadyLearn.Common.Abstractions.Messaging;

/// <summary>
/// Command to refresh access token using refresh token.
/// </summary>
public record RefreshTokenCommand : ICommand<RefreshTokenResponse>
{
    public required string RefreshToken { get; init; }
}

/// <summary>
/// Response returned after successful token refresh.
/// </summary>
public record RefreshTokenResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
}
