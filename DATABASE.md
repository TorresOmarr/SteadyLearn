# 🗄️ DATABASE.md - EF Core & Migrations

**Status**: Foundation Phase
**Last Updated**: January 10, 2025

---

## 📌 Overview

The database uses:
- **ORM**: Entity Framework Core 8
- **Database**: PostgreSQL
- **Migrations**: Code-First approach
- **Strategy**: Soft delete + Auditing + Translations

---

## 🏗️ Database Context

### ApplicationDbContext Structure
```csharp
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Entity Sets
    public DbSet<User> Users { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CourseTranslation> CourseTranslations { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<TopicTranslation> TopicTranslations { get; set; }
    public DbSet<Subtopic> Subtopics { get; set; }
    public DbSet<SubtopicTranslation> SubtopicTranslations { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<SessionTranslation> SessionTranslations { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Apply all configurations from assembly
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Global query filters for soft delete
        builder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Course>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Topic>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Subtopic>().HasQueryFilter(x => !x.IsDeleted);
        builder.Entity<Session>().HasQueryFilter(x => !x.IsDeleted);

        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add audit information before saving
        AddAuditInformation();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddAuditInformation()
    {
        var entries = ChangeTracker.Entries<IAuditableEntity>();
        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = now;
                entry.Entity.CreatedBy = GetCurrentUser();
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = now;
                entry.Entity.UpdatedBy = GetCurrentUser();
            }
        }
    }

    private string GetCurrentUser()
    {
        // Return current user email or "SYSTEM" if not authenticated
        return "system"; // TODO: Implement based on HttpContext.User
    }
}
```

---

## 🔧 Entity Configurations

### Configuration Pattern
```csharp
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        // Table name
        builder.ToTable("Courses");

        // Primary key
        builder.HasKey(x => x.Id);

        // Properties
        builder.Property(x => x.Id)
            .ValueGeneratedNever(); // GUID provided by application

        builder.Property(x => x.IsPublished)
            .HasDefaultValue(false);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(x => x.TotalDurationMinutes)
            .HasDefaultValue(0);

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.UpdatedAtUtc)
            .IsRequired(false);

        builder.Property(x => x.UpdatedBy)
            .HasMaxLength(255)
            .IsRequired(false);

        // Relationships
        builder.HasMany(x => x.Topics)
            .WithOne()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CourseTranslations)
            .WithOne()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.IsDeleted, x.IsPublished });
    }
}
```

### UserConfiguration
```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        // Unique constraint on email (case-insensitive)
        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasFilter("LOWER(\"Email\")")
            .HasDatabaseName("IX_Users_Email_Unique");

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Role)
            .HasConversion<string>();

        builder.Property(x => x.IsEmailVerified)
            .HasDefaultValue(false);

        builder.Property(x => x.EmailVerificationToken)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.EmailVerificationTokenExpiresAt)
            .IsRequired(false);

        builder.Property(x => x.PasswordResetToken)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.PasswordResetTokenExpiresAt)
            .IsRequired(false);

        builder.Property(x => x.RefreshTokenHash)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(x => x.RefreshTokenExpiresAt)
            .IsRequired(false);

        builder.Property(x => x.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(x => new { x.Role, x.IsEmailVerified });
    }
}
```

### CourseTranslationConfiguration
```csharp
public class CourseTranslationConfiguration : IEntityTypeConfiguration<CourseTranslation>
{
    public void Configure(EntityTypeBuilder<CourseTranslation> builder)
    {
        builder.ToTable("CourseTranslations");

        builder.HasKey(x => x.Id);

        // Foreign key
        builder.HasOne<Course>()
            .WithMany(x => x.CourseTranslations)
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CourseId).IsRequired();

        builder.Property(x => x.LanguageCode)
            .IsRequired()
            .HasMaxLength(5); // en, es, fr, etc.

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Description)
            .IsRequired();

        // Unique constraint: Only one translation per language per course
        builder.HasIndex(x => new { x.CourseId, x.LanguageCode })
            .IsUnique()
            .HasDatabaseName("IX_CourseTranslations_CourseId_Language_Unique");
    }
}
```

---

## 📊 Schema Overview

### Tables Created
```
Users
├── PK: Id (GUID)
├── Unique: Email
├── Columns: 15+ (auth, audit, verification)
└── Indexes: 2

Courses
├── PK: Id (GUID)
├── FK: None (root aggregate)
├── Columns: 6 (published, duration, audit, delete)
└── Indexes: 1

CourseTranslations
├── PK: Id (GUID)
├── FK: CourseId
├── Unique: (CourseId, LanguageCode)
├── Columns: 4 (title, description, language)
└── Indexes: 2

Topics
├── PK: Id (GUID)
├── FK: CourseId (required, no change)
├── Columns: 6 (order, audit, delete)
└── Indexes: 2

TopicTranslations
├── PK: Id (GUID)
├── FK: TopicId
├── Unique: (TopicId, LanguageCode)
└── Columns: 3

Subtopics
├── PK: Id (GUID)
├── FK: TopicId (required, no change)
├── Columns: 6 (order, audit, delete)
└── Indexes: 2

SubtopicTranslations
├── PK: Id (GUID)
├── FK: SubtopicId
├── Unique: (SubtopicId, LanguageCode)
└── Columns: 3

Sessions
├── PK: Id (GUID)
├── FK: SubtopicId (can change via move)
├── Columns: 9 (video, markdown, duration, source, order)
└── Indexes: 2

SessionTranslations
├── PK: Id (GUID)
├── FK: SessionId
├── Unique: (SessionId, LanguageCode)
└── Columns: 4 (title, content markdown, language)
```

---

## 🔄 Migrations Workflow

### Creating a Migration
```bash
# Navigate to API folder
cd api

# Create migration
dotnet ef migrations add InitialCreate

# Or with descriptive name
dotnet ef migrations add AddUserEmailVerification

# This creates:
#   Migrations/[timestamp]_[name].cs (Up/Down methods)
#   Migrations/ApplicationDbContextModelSnapshot.cs (current state)
```

### Applying Migrations
```bash
# Update database to latest
dotnet ef database update

# Update to specific migration
dotnet ef database update InitialCreate

# Revert to previous (destructive!)
dotnet ef database update PreviousMigration
```

### Migration Best Practices
```
✓ One logical change per migration
✓ Descriptive names (AddUserTable, AddEmailVerification)
✓ Always test migrations locally
✓ Never skip migrations in production
✓ Keep migrations small and focused
✓ Document data transformations
```

---

## 🌱 Database Seeding

### Seeding Pattern
```csharp
// Program.cs
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Run migrations
    await context.Database.MigrateAsync();
    
    // Seed data
    await context.SeedAdminAsync(builder.Configuration);
    await context.SeedInitialCoursesAsync();
}

app.Run();
```

### Seeding Methods
```csharp
public static async Task SeedAdminAsync(
    this ApplicationDbContext context,
    IConfiguration config)
{
    if (await context.Users.AnyAsync(x => x.Role == UserRole.Admin))
        return;

    var admin = new User
    {
        Id = Guid.NewGuid(),
        Email = config["Admin:Email"],
        FirstName = config["Admin:FirstName"],
        LastName = config["Admin:LastName"],
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(config["Admin:Password"]),
        Role = UserRole.Admin,
        IsEmailVerified = true,
        CreatedAtUtc = DateTime.UtcNow,
        CreatedBy = "SYSTEM"
    };

    context.Users.Add(admin);
    await context.SaveChangesAsync();
}
```

---

## 🔍 Query Examples

### Get Course with All Content
```csharp
var course = await _db.Courses
    .Include(x => x.CourseTranslations
        .Where(t => t.LanguageCode == language))
    .Include(x => x.Topics)
    .ThenInclude(t => t.TopicTranslations
        .Where(tr => tr.LanguageCode == language))
    .ThenInclude(t => t.Subtopics)
    .ThenInclude(s => s.SubtopicTranslations
        .Where(str => str.LanguageCode == language))
    .ThenInclude(s => s.Sessions)
    .ThenInclude(se => se.SessionTranslations
        .Where(set => set.LanguageCode == language))
    .FirstOrDefaultAsync(x => x.Id == courseId);
```

### Get All Published Courses (with pagination)
```csharp
var courses = await _db.Courses
    .Where(x => x.IsPublished)
    .Include(x => x.CourseTranslations
        .Where(t => t.LanguageCode == language))
    .OrderByDescending(x => x.CreatedAtUtc)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### Calculate Course Duration
```csharp
var totalMinutes = await _db.Sessions
    .Where(x => x.Subtopic.Topic.CourseId == courseId
        && !x.IsDeleted
        && x.DurationMinutes.HasValue)
    .SumAsync(x => (int?)x.DurationMinutes) ?? 0;
```

---

## 🛡️ Soft Delete Implementation

### Global Query Filter
```csharp
// Applied in DbContext.OnModelCreating
builder.Entity<Course>().HasQueryFilter(x => !x.IsDeleted);
builder.Entity<Topic>().HasQueryFilter(x => !x.IsDeleted);
builder.Entity<Subtopic>().HasQueryFilter(x => !x.IsDeleted);
builder.Entity<Session>().HasQueryFilter(x => !x.IsDeleted);
builder.Entity<User>().HasQueryFilter(x => !x.IsDeleted);
```

### Soft Delete Logic
```csharp
// Handler for DeleteCourseCommand
public async Task<Result> Handle(DeleteCourseCommand request, CancellationToken ct)
{
    var course = await _db.Courses.FirstOrDefaultAsync(x => x.Id == request.CourseId, ct);
    
    if (course == null)
        return Result.Failure("COURSE_NOT_FOUND");
    
    // Mark as deleted (don't remove from DB)
    course.IsDeleted = true;
    course.UpdatedAtUtc = DateTime.UtcNow;
    course.UpdatedBy = GetCurrentUser();
    
    _db.Courses.Update(course);
    await _db.SaveChangesAsync(ct);
    
    return Result.Success();
}

// Query automatically excludes deleted courses
var courses = await _db.Courses.ToListAsync(); // IsDeleted = false
```

### Permanently Delete (Rare)
```csharp
// If needed: GDPR, data cleanup, etc.
var course = await _db.Courses
    .IgnoreQueryFilters() // Bypass global filter
    .FirstOrDefaultAsync(x => x.Id == courseId);

_db.Courses.Remove(course); // Hard delete
await _db.SaveChangesAsync();
```

---

## 📈 Indexes Strategy

### Why Indexes?
```
✓ Speed up WHERE clauses
✓ Speed up ORDER BY
✓ Speed up JOINs
✓ Cost: Storage + Write performance

✗ Too many indexes slow down inserts
✗ Too many indexes waste storage
```

### Indexing Strategy
```
Index on frequently queried columns:
  ✓ Course: (IsDeleted, IsPublished)
  ✓ Topic: (CourseId, Order)
  ✓ Subtopic: (TopicId, Order)
  ✓ Session: (SubtopicId, Order)
  ✓ User: Email (unique)

Composite indexes for common queries:
  ✓ (CourseId, LanguageCode) on translations
  ✓ (Role, IsEmailVerified) on users
```

---

## 🔗 Connection String

### PostgreSQL Local (Docker)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=steadylearn;User Id=postgres;Password=postgres;"
  }
}
```

### Development (docker-compose)
```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15-alpine
    environment:
      POSTGRES_DB: steadylearn
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

---

## ✅ Database Checklist

Before deployment:
- [ ] All entities configured with IEntityTypeConfiguration
- [ ] All foreign keys defined
- [ ] Indexes on frequently queried columns
- [ ] Unique constraints where needed
- [ ] Global query filters for soft delete
- [ ] Audit fields populated automatically
- [ ] Migrations tested locally
- [ ] Seeding works correctly
- [ ] No circular references
- [ ] All translations seeded

---

## 🔗 Related Documents

- **ARCHITECTURE.md** - DbContext in context
- **DOMAIN_MODEL.md** - Entity definitions
- **AUTH_IMPLEMENTATION.md** - User schema
- **I18N_STRATEGY.md** - Translation tables
- **AGENTS.md** - Overall database strategy

---

*A well-designed database schema is the foundation of a maintainable application.*
