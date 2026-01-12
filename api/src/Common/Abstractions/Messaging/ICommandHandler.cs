namespace SteadyLearn.Common.Abstractions.Messaging;

using MediatR;
using SteadyLearn.Common.Abstractions;

/// <summary>
/// Represents a handler for a command that returns a Result (no response data).
/// </summary>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Represents a handler for a command that returns a Result with response data.
/// </summary>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
