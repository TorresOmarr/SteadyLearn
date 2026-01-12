namespace SteadyLearn.Modules.Auth.Register;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SteadyLearn.Common.Abstractions;
using SteadyLearn.Common.Abstractions.Messaging;
using SteadyLearn.Common.Constants;
using SteadyLearn.Common.Options;
using SteadyLearn.Common.Security;
using SteadyLearn.Data;
using SteadyLearn.Domain.Entities;

/// <summary>
/// Handler for user registration.
/// </summary>
public class RegisterCommandHandler : ICommandHandler<RegisterCommand, RegisterResponse>
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly IEmailService _emailService;
    private readonly EmailOptions _emailOptions;

    public RegisterCommandHandler(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenProvider jwtTokenProvider,
        IEmailService emailService,
        IOptions<EmailOptions> emailOptions)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenProvider = jwtTokenProvider;
        _emailService = emailService;
        _emailOptions = emailOptions.Value ?? throw new InvalidOperationException("EmailOptions are not configured");
    }

    public async Task<Result<RegisterResponse>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        // Check if email already exists
        var emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower(), cancellationToken);

        if (emailExists)
        {
            return Result.Failure<RegisterResponse>(
                ErrorCodes.EmailAlreadyExists,
                "A user with this email already exists");
        }

        // Generate email verification token
        var verificationToken = _jwtTokenProvider.GenerateSecureToken();
        var verificationTokenHash = _jwtTokenProvider.HashToken(verificationToken);
        var verificationTokenExpiry = _emailOptions.VerificationTokenExpirationHours;

        // Create user
        var user = User.Create(
            request.Email,
            _passwordHasher.Hash(request.Password),
            request.FirstName,
            request.LastName,
            UserRole.Student);

        user.SetEmailVerificationToken(verificationTokenHash, DateTimeOffset.UtcNow.AddHours(verificationTokenExpiry));

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        // Send verification email (async, fire-and-forget)
        _ = _emailService.SendEmailVerificationAsync(user.Email, verificationToken);

        return Result.Success(new RegisterResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Message = "Registration successful. Please check your email to verify your account."
        });
    }
}
