namespace SteadyLearn.Data.Seeders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SteadyLearn.Common.Options;
using SteadyLearn.Common.Security;
using SteadyLearn.Domain.Entities;

/// <summary>
/// Seeds the initial admin user on application startup.
/// </summary>
public class AdminSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly AdminOptions _options;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IOptions<AdminOptions> options,
        ILogger<AdminSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _options = options.Value ?? throw new InvalidOperationException("Admin options are not configured");
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        var adminEmail = _options.Email;
        var adminPassword = _options.Password;

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            _logger.LogWarning("Admin credentials not configured. Skipping admin seeding.");
            return;
        }

        // Check if admin already exists
        var adminExists = await _context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email.ToLower() == adminEmail.ToLower());

        if (adminExists)
        {
            _logger.LogInformation("Admin user already exists. Skipping seeding.");
            return;
        }

        // Create admin user
        var admin = User.Create(
            adminEmail,
            _passwordHasher.Hash(adminPassword),
            "Admin",
            "User",
            UserRole.Admin);

        admin.MarkEmailAsVerified();

        _context.Users.Add(admin);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Admin user created successfully with email: {Email}", adminEmail);
    }
}
