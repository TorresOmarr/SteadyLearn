namespace SteadyLearn.Modules.Auth.Register;

using FluentValidation;
using SteadyLearn.Common.Constants;

/// <summary>
/// Validator for RegisterCommand.
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.FieldRequired)
            .WithMessage("Email is required")
            .EmailAddress()
            .WithErrorCode(ErrorCodes.InvalidEmailFormat)
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithErrorCode(ErrorCodes.FieldTooLong)
            .WithMessage("Email must be 255 characters or less");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.FieldRequired)
            .WithMessage("Password is required")
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

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.FieldTooLong)
            .WithMessage("First name must be 100 characters or less")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.FieldTooLong)
            .WithMessage("Last name must be 100 characters or less")
            .When(x => x.LastName != null);
    }
}
