namespace SteadyLearn.Common.Abstractions.Messaging;

using MediatR;
using SteadyLearn.Common.Abstractions;

/// <summary>
/// Represents a query that returns a Result with response data of type TResponse.
/// Queries represent read operations that don't change system state.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
