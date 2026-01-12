namespace SteadyLearn.Data;

using Microsoft.EntityFrameworkCore;
using SteadyLearn.Domain.Entities;
using SteadyLearn.Data.Configurations;

/// <summary>
/// Main database context for SteadyLearn.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Register all entity configurations
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        // Global query filters for soft deletes
        // Only query non-deleted entities by default
        var deletableEntityType = typeof(User);
        var method = typeof(ApplicationDbContext)
            .GetMethod(nameof(GetDeletedFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
            .MakeGenericMethod(deletableEntityType);

        if (method != null)
        {
            var filter = method.Invoke(null, new object[] { }) as System.Linq.Expressions.LambdaExpression;
            if (filter != null)
            {
                modelBuilder.Entity<User>().HasQueryFilter(filter);
            }
        }
    }

    private static System.Linq.Expressions.Expression<Func<TEntity, bool>> GetDeletedFilter<TEntity>()
        where TEntity : class, Domain.Interfaces.IAuditableEntity
    {
        System.Linq.Expressions.Expression<Func<TEntity, bool>> filter = x => x.DeletedAt == null;
        return filter;
    }
}
