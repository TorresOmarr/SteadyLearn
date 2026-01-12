namespace SteadyLearn.Common.Extensions;

using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SteadyLearn.Common.Behaviors;
using SteadyLearn.Common.Options;
using SteadyLearn.Common.Security;
using SteadyLearn.Data;
using SteadyLearn.Data.Seeders;

/// <summary>
/// Extension methods for service collection configuration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds database services.
    /// </summary>
    /// <summary>
    /// Adds database services using typed options.
    /// </summary>
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            var dbOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value
                ?? throw new InvalidOperationException("DatabaseOptions are not configured");

            options.UseNpgsql(dbOptions.DefaultConnection).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<AdminSeeder>();

        return services;
    }

    /// <summary>
    /// Adds authentication and authorization services.
    /// </summary>
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        // Security services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IEmailService, MockEmailService>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer((options) =>
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var sp = scope.ServiceProvider;
            var jwt = sp.GetRequiredService<IOptions<JwtOptions>>().Value
                ?? throw new InvalidOperationException("JwtOptions are not configured");

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt.Issuer,
                ValidAudience = jwt.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
                ClockSkew = TimeSpan.Zero // No clock skew for testing
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy =>
                policy.RequireRole("Admin"));

            options.AddPolicy("StudentOnly", policy =>
                policy.RequireRole("Student"));
        });

        return services;
    }

    /// <summary>
    /// Adds MediatR and validation services.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        return services;
    }
}
