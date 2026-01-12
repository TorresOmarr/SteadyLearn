namespace SteadyLearn.Common.Abstractions.Messaging;

using MediatR;
using SteadyLearn.Common.Abstractions;

/// <summary>
/// Represents a handler for a query that returns a Result with response data.
/// </summary>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
