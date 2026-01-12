namespace SteadyLearn.Common.Abstractions.Messaging;

using MediatR;
using SteadyLearn.Common.Abstractions;

/// <summary>
/// Represents a command that returns a Result (no response data).
/// </summary>
public interface ICommand : IRequest<Result>, IBaseCommand
{
}

/// <summary>
/// Represents a command that returns a Result with response data of type TResponse.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand
{
}
