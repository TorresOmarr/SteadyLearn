namespace SteadyLearn.Modules.Auth.Login;

using SteadyLearn.Common.Abstractions.Messaging;

/// <summary>
/// Command to login a user.
/// </summary>
public record LoginCommand : ICommand<LoginResponse>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

/// <summary>
/// Response returned after successful login.
/// </summary>
public record LoginResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required UserDto User { get; init; }
}

/// <summary>
/// User data transfer object.
/// </summary>
public record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public required string Role { get; init; }
}
