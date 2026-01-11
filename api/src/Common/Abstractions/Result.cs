namespace SteadyLearn.Common.Abstractions;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// This is a functional approach to error handling that avoids exceptions for business logic.
/// 
/// Example:
///   var result = await handler.Handle(command);
///   if (result.IsFailure)
///       return BadRequest(new { result.Error.Code, result.Error.Message });
///   return Ok(result.Value);
/// </summary>
public abstract record Result
{
    /// <summary>
    /// Creates a successful result with no value.
    /// </summary>
    public static Result Success() => new SuccessResult();

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<T> Success<T>(T value) => new SuccessResult<T>(value);

    /// <summary>
    /// Creates a failed result with an error.
    /// </summary>
    public static Result Failure(Error error) => new FailureResult(error);

    /// <summary>
    /// Creates a failed result with an error.
    /// </summary>
    public static Result<T> Failure<T>(Error error) => new FailureResult<T>(error);

    /// <summary>
    /// Creates a failed result with error code and optional message.
    /// </summary>
    public static Result Failure(string errorCode, string? message = null)
        => new FailureResult(Error.Create(errorCode, message));

    /// <summary>
    /// Creates a failed result with error code and optional message.
    /// </summary>
    public static Result<T> Failure<T>(string errorCode, string? message = null)
        => new FailureResult<T>(Error.Create(errorCode, message));

    public abstract TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure);

    public abstract Task<TResult> MatchAsync<TResult>(
        Func<Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure);

    public abstract void Match(
        Action onSuccess,
        Action<Error> onFailure);

    public abstract Task MatchAsync(
        Func<Task> onSuccess,
        Func<Error, Task> onFailure);

    public abstract bool IsSuccess { get; }
    public abstract bool IsFailure { get; }

    // Success Result (no value)
    private sealed record SuccessResult : Result
    {
        public override bool IsSuccess => true;
        public override bool IsFailure => false;

        public override TResult Match<TResult>(
            Func<TResult> onSuccess,
            Func<Error, TResult> onFailure) => onSuccess();

        public override async Task<TResult> MatchAsync<TResult>(
            Func<Task<TResult>> onSuccess,
            Func<Error, Task<TResult>> onFailure) => await onSuccess();

        public override void Match(
            Action onSuccess,
            Action<Error> onFailure) => onSuccess();

        public override async Task MatchAsync(
            Func<Task> onSuccess,
            Func<Error, Task> onFailure) => await onSuccess();
    }

    // Failure Result
    private sealed record FailureResult : Result
    {
        private readonly Error _error;

        public override bool IsSuccess => false;
        public override bool IsFailure => true;

        public FailureResult(Error error)
        {
            _error = error;
        }

        public override TResult Match<TResult>(
            Func<TResult> onSuccess,
            Func<Error, TResult> onFailure) => onFailure(_error);

        public override async Task<TResult> MatchAsync<TResult>(
            Func<Task<TResult>> onSuccess,
            Func<Error, Task<TResult>> onFailure) => await onFailure(_error);

        public override void Match(
            Action onSuccess,
            Action<Error> onFailure) => onFailure(_error);

        public override async Task MatchAsync(
            Func<Task> onSuccess,
            Func<Error, Task> onFailure) => await onFailure(_error);
    }
}

/// <summary>
/// Represents the result of an operation that can either succeed with a value of type T or fail with an error.
/// </summary>
public abstract record Result<T>
{
    public abstract TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure);

    public abstract Task<TResult> MatchAsync<TResult>(
        Func<T, Task<TResult>> onSuccess,
        Func<Error, Task<TResult>> onFailure);

    public abstract void Match(
        Action<T> onSuccess,
        Action<Error> onFailure);

    public abstract Task MatchAsync(
        Func<T, Task> onSuccess,
        Func<Error, Task> onFailure);

    public abstract bool IsSuccess { get; }
    public abstract bool IsFailure { get; }
    public abstract T? Value { get; }
    public abstract Error? Error { get; }

    // Success Result with Value
    private sealed record SuccessResult : Result<T>
    {
        private readonly T _value;

        public override bool IsSuccess => true;
        public override bool IsFailure => false;
        public override T? Value => _value;
        public override Error? Error => null;

        public SuccessResult(T value)
        {
            _value = value;
        }

        public override TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<Error, TResult> onFailure) => onSuccess(_value);

        public override async Task<TResult> MatchAsync<TResult>(
            Func<T, Task<TResult>> onSuccess,
            Func<Error, Task<TResult>> onFailure) => await onSuccess(_value);

        public override void Match(
            Action<T> onSuccess,
            Action<Error> onFailure) => onSuccess(_value);

        public override async Task MatchAsync(
            Func<T, Task> onSuccess,
            Func<Error, Task> onFailure) => await onSuccess(_value);
    }

    // Failure Result with Error
    private sealed record FailureResult : Result<T>
    {
        private readonly Error _error;

        public override bool IsSuccess => false;
        public override bool IsFailure => true;
        public override T? Value => default;
        public override Error? Error => _error;

        public FailureResult(Error error)
        {
            _error = error;
        }

        public override TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<Error, TResult> onFailure) => onFailure(_error);

        public override async Task<TResult> MatchAsync<TResult>(
            Func<T, Task<TResult>> onSuccess,
            Func<Error, Task<TResult>> onFailure) => await onFailure(_error);

        public override void Match(
            Action<T> onSuccess,
            Action<Error> onFailure) => onFailure(_error);

        public override async Task MatchAsync(
            Func<T, Task> onSuccess,
            Func<Error, Task> onFailure) => await onFailure(_error);
    }
}
