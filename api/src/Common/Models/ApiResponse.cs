namespace SteadyLearn.Common.Models;

/// <summary>
/// Standard API response wrapper for successful responses.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; } = true;
    public T? Data { get; init; }
    public string? Message { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ApiResponse<T> Ok(string message) => new()
    {
        Success = true,
        Message = message
    };
}

/// <summary>
/// Standard API response for error responses.
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; init; } = false;
    public required string Code { get; init; }
    public string? Message { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }

    public static ApiErrorResponse FromError(string code, string? message = null) => new()
    {
        Success = false,
        Code = code,
        Message = message
    };

    public static ApiErrorResponse FromValidationErrors(Dictionary<string, string[]> errors) => new()
    {
        Success = false,
        Code = "VALIDATION_ERROR",
        Message = "One or more validation errors occurred.",
        Errors = errors
    };
}
