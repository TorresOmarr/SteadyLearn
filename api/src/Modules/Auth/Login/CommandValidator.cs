namespace SteadyLearn.Modules.Auth.Login;

using FluentValidation;
using SteadyLearn.Common.Constants;

/// <summary>
/// Validator for LoginCommand.
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.FieldRequired)
            .WithMessage("Email is required")
            .EmailAddress()
            .WithErrorCode(ErrorCodes.InvalidEmailFormat)
            .WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.FieldRequired)
            .WithMessage("Password is required");
    }
}
