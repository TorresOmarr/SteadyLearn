namespace SteadyLearn.Common.Behaviors;

using FluentValidation;
using MediatR;
using SteadyLearn.Common.Abstractions;
using SteadyLearn.Common.Constants;

/// <summary>
/// MediatR pipeline behavior that validates requests using FluentValidation.
/// Runs before the handler is executed.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            // Return the first validation error
            var firstError = failures.First();
            var errorCode = firstError.ErrorCode ?? ErrorCodes.InvalidInput;
            var errorMessage = firstError.ErrorMessage;

            // Create a failure result - we need to handle both Result and Result<T>
            return CreateFailureResult(errorCode, errorMessage);
        }

        return await next();
    }

    private static TResponse CreateFailureResult(string errorCode, string errorMessage)
    {
        var error = Error.Create(errorCode, errorMessage);

        // Check if TResponse is Result<T> or just Result
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(error);
        }

        // For Result<T>, we need to use reflection or a different approach
        // Get the generic type argument from Result<T>
        var resultType = typeof(TResponse);
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var genericArg = resultType.GetGenericArguments()[0];
            var failureMethod = typeof(Result).GetMethod(nameof(Result.Failure), 1, new[] { typeof(Error) });
            var genericFailureMethod = failureMethod?.MakeGenericMethod(genericArg);
            var result = genericFailureMethod?.Invoke(null, new object[] { error });
            return (TResponse)result!;
        }

        throw new InvalidOperationException($"Cannot create failure result for type {typeof(TResponse)}");
    }
}
