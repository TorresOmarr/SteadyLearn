namespace SteadyLearn.Modules.Auth.Register;

using SteadyLearn.Common.Abstractions.Messaging;

/// <summary>
/// Command to register a new user.
/// </summary>
public record RegisterCommand : ICommand<RegisterResponse>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
}

/// <summary>
/// Response returned after successful registration.
/// </summary>
public record RegisterResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string Message { get; init; }
}
