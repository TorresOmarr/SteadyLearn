namespace SteadyLearn.Modules.Auth.VerifyEmail;

using FluentValidation;
using SteadyLearn.Common.Constants;

/// <summary>
/// Validator for VerifyEmailCommand.
/// </summary>
public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.FieldRequired)
            .WithMessage("Verification token is required");
    }
}
