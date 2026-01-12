namespace SteadyLearn.Modules.Auth.RefreshToken;

using FluentValidation;
using SteadyLearn.Common.Constants;

/// <summary>
/// Validator for RefreshTokenCommand.
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.FieldRequired)
            .WithMessage("Refresh token is required");
    }
}
