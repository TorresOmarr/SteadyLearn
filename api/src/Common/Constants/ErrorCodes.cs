namespace SteadyLearn.Common.Constants;

/// <summary>
/// Language-agnostic error codes used throughout the application.
/// Frontend is responsible for translating these codes to user-friendly messages.
/// </summary>
public static class ErrorCodes
{
    // Authentication Errors
    public const string EmailAlreadyExists = "EMAIL_ALREADY_EXISTS";
    public const string InvalidEmailFormat = "INVALID_EMAIL_FORMAT";
    public const string InvalidPassword = "INVALID_PASSWORD";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string UserNotFound = "USER_NOT_FOUND";
    public const string AccountNotVerified = "ACCOUNT_NOT_VERIFIED";
    public const string InvalidToken = "INVALID_TOKEN";
    public const string TokenExpired = "TOKEN_EXPIRED";
    public const string RefreshTokenNotFound = "REFRESH_TOKEN_NOT_FOUND";

    // Validation Errors
    public const string InvalidInput = "INVALID_INPUT";
    public const string FieldRequired = "FIELD_REQUIRED";
    public const string FieldTooLong = "FIELD_TOO_LONG";
    public const string FieldTooShort = "FIELD_TOO_SHORT";

    // Authorization Errors
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";
    public const string InsufficientPermissions = "INSUFFICIENT_PERMISSIONS";

    // Email Errors
    public const string EmailSendFailed = "EMAIL_SEND_FAILED";
    public const string EmailAlreadyVerified = "EMAIL_ALREADY_VERIFIED";
    public const string VerificationTokenExpired = "VERIFICATION_TOKEN_EXPIRED";
    public const string InvalidVerificationToken = "INVALID_VERIFICATION_TOKEN";

    // Course Errors
    public const string CourseNotFound = "COURSE_NOT_FOUND";
    public const string CourseAlreadyExists = "COURSE_ALREADY_EXISTS";
    public const string TopicNotFound = "TOPIC_NOT_FOUND";
    public const string SubtopicNotFound = "SUBTOPIC_NOT_FOUND";
    public const string SessionNotFound = "SESSION_NOT_FOUND";

    // Server Errors
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string DatabaseError = "DATABASE_ERROR";
    public const string OperationFailed = "OPERATION_FAILED";
}
