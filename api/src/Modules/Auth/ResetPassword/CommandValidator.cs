namespace SteadyLearn.Modules.Auth.ResetPassword;

using FluentValidation;
using SteadyLearn.Common.Constants;

/// <summary>
/// Validator for RequestPasswordResetCommand.
/// </summary>
public class RequestPasswordResetCommandValidator : AbstractValidator<RequestPasswordResetCommand>
{
    public RequestPasswordResetCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.FieldRequired)
            .WithMessage("Email is required")
            .EmailAddress()
            .WithErrorCode(ErrorCodes.InvalidEmailFormat)
            .WithMessage("Invalid email format");
    }
}

/// <summary>
/// Validator for CompletePasswordResetCommand.
/// </summary>
public class CompletePasswordResetCommandValidator : AbstractValidator<CompletePasswordResetCommand>
{
    public CompletePasswordResetCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.FieldRequired)
            .WithMessage("Reset token is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.FieldRequired)
            .WithMessage("New password is required")
            .MinimumLength(8)
            .WithErrorCode(ErrorCodes.FieldTooShort)
            .WithMessage("Password must be at least 8 characters")
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.FieldTooLong)
            .WithMessage("Password must be 100 characters or less")
            .Matches("[A-Z]")
            .WithErrorCode(ErrorCodes.InvalidPassword)
            .WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]")
            .WithErrorCode(ErrorCodes.InvalidPassword)
            .WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]")
            .WithErrorCode(ErrorCodes.InvalidPassword)
            .WithMessage("Password must contain at least one number")
            .Matches("[^a-zA-Z0-9]")
            .WithErrorCode(ErrorCodes.InvalidPassword)
            .WithMessage("Password must contain at least one special character");
    }
}
