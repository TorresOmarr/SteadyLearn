# 🏗️ ARCHITECTURE.md - Backend Structure & Patterns

**Status**: Foundation Phase
**Last Updated**: January 10, 2025

---

## 📐 Vertical Slice Architecture

The backend follows **Vertical Slice Architecture**, where each feature is self-contained with its own folder structure. This ensures:
- No shared business logic between features
- Easy to test in isolation
- Easy to delete or extend without side effects
- Clear ownership of each feature

---

## 📁 Folder Structure

```
/api
├── src/
│   ├── Modules/                    # Feature slices
│   │   ├── Auth/
│   │   │   ├── Register/
│   │   │   │   ├── Endpoint.cs
│   │   │   │   ├── Command.cs
│   │   │   │   ├── CommandValidator.cs
│   │   │   │   ├── CommandHandler.cs
│   │   │   │   └── Tests/
│   │   │   │       ├── RegisterCommandHandlerTests.cs
│   │   │   │       └── RegisterValidatorTests.cs
│   │   │   ├── Login/
│   │   │   ├── Logout/
│   │   │   ├── RefreshToken/
│   │   │   ├── VerifyEmail/
│   │   │   └── ResetPassword/
│   │   ├── Courses/
│   │   │   ├── CreateCourse/
│   │   │   ├── GetCourse/
│   │   │   ├── GetAllCourses/
│   │   │   ├── UpdateCourse/
│   │   │   ├── PublishCourse/
│   │   │   ├── DeleteCourse/
│   │   │   └── Tests/
│   │   ├── Topics/
│   │   ├── Subtopics/
│   │   └── Sessions/
│   ├── Common/                     # Shared utilities
│   │   ├── Models/
│   │   │   ├── Result.cs           # Result<T> pattern
│   │   │   ├── ApiResponse.cs
│   │   │   └── ErrorResponse.cs
│   │   ├── Behaviors/
│   │   │   ├── ValidationBehavior.cs
│   │   │   └── LoggingBehavior.cs
│   │   ├── Middleware/
│   │   │   ├── ErrorHandlingMiddleware.cs
│   │   │   ├── AcceptLanguageMiddleware.cs
│   │   │   └── AuthMiddleware.cs
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── MediatorExtensions.cs
│   │   ├── Constants/
│   │   │   ├── ErrorCodes.cs
│   │   │   └── ValidationMessages.cs
│   │   └── Utils/
│   ├── Domain/                     # Domain model
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Course.cs
│   │   │   ├── CourseTranslation.cs
│   │   │   ├── Topic.cs
│   │   │   ├── Subtopic.cs
│   │   │   └── Session.cs
│   │   ├── Interfaces/
│   │   │   ├── IAuditableEntity.cs
│   │   │   ├── ITranslatable.cs
│   │   │   └── IDeletableEntity.cs
│   │   └── Exceptions/
│   │       └── DomainException.cs
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/         # EF Core entity configs
│   │   │   ├── UserConfiguration.cs
│   │   │   ├── CourseConfiguration.cs
│   │   │   └── ...
│   │   └── Migrations/
│   │       └── [auto-generated]
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Program.cs
│   └── api.csproj
├── Tests/                          # Unit tests (optional separate folder)
│   └── SteadyLearn.Tests.csproj
└── docker-compose.yml
```

---

## 🎯 Feature Slice Anatomy

Each feature slice follows this pattern:

### 1. **Command/Query**
```
Represents the input contract for the operation.
Contains only input data, no behavior.
```

### 2. **Validator**
```
FluentValidation validator for the Command/Query.
Handles multilenguaje error messages.
Can be reused across multiple handlers.
```

### 3. **Handler**
```
Contains business logic.
Should be:
  - Pure (no side effects)
  - Testable (use interfaces for dependencies)
  - Single-responsibility
```

### 4. **Endpoint**
```
Maps HTTP request → Command → Handler → Response
Uses MediatR to invoke handlers.
Returns properly formatted API response.
```

### 5. **Tests**
```
Unit tests for Handler + Validator.
Use InMemory DB or Testcontainers.
No mocking of business logic.
```

---

## 📦 Key Patterns

### 1. **Result<T> Pattern**
```
Used for explicit error handling without exceptions.

Result<T>
├── Success(data: T)
└── Failure(code: string, message?: string)

Returns Result, never throws for business logic.
```

### 2. **MediatR Pipeline**
```
Request → ValidationBehavior → Handler → Response
         → LoggingBehavior
         → ExceptionHandlingBehavior
```

### 3. **DTOs vs Entities**
```
Entities: Live in Domain/Entities, used in DB
DTOs: Live in features, used for API contracts

Never expose entities directly in API responses.
```

### 4. **Soft Delete**
```
All entities inherit from IAuditableEntity:
  - CreatedAtUtc
  - CreatedBy
  - UpdatedAtUtc
  - UpdatedBy
  - IsDeleted (soft delete)

Global query filter: .Where(x => !x.IsDeleted)
```

### 5. **Auditing**
```
All entities track:
  - Who created/updated
  - When they were created/updated
  - Auto-populated via middleware or interceptors
```

---

## 🔄 Workflow: Implementing a Feature

### Step 1: Create the Slice Folder
```
/Modules/Courses/CreateCourse/
  ├── Endpoint.cs
  ├── Command.cs
  ├── CommandValidator.cs
  ├── CommandHandler.cs
  └── Tests/
      └── CreateCourseCommandHandlerTests.cs
```

### Step 2: Define Command (Input)
```csharp
// Command.cs
public record CreateCourseCommand(
    string TitleEn,
    string TitleEs,
    string DescriptionEn,
    string DescriptionEs
) : IRequest<Result<CreateCourseResponse>>;
```

### Step 3: Create Validator
```csharp
// CommandValidator.cs
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.TitleEn).NotEmpty().WithMessage("en:COURSE_TITLE_REQUIRED");
        RuleFor(x => x.TitleEs).NotEmpty().WithMessage("es:COURSE_TITLE_REQUIRED");
    }
}
```

### Step 4: Implement Handler
```csharp
// CommandHandler.cs
public class CreateCourseCommandHandler : IRequestHandler<CreateCourseCommand, Result<CreateCourseResponse>>
{
    private readonly ApplicationDbContext _db;
    
    public async Task<Result<CreateCourseResponse>> Handle(...)
    {
        // Business logic
        var course = new Course { /* ... */ };
        _db.Courses.Add(course);
        await _db.SaveChangesAsync();
        
        return Result.Success(new CreateCourseResponse(course.Id));
    }
}
```

### Step 5: Create Endpoint
```csharp
// Endpoint.cs
public static void MapCreateCourseEndpoint(this WebApplication app)
{
    app.MapPost("/api/courses", CreateCourseAsync)
        .WithName("CreateCourse")
        .WithOpenApi()
        .RequireAuthorization("AdminOnly");
}

private static async Task<IResult> CreateCourseAsync(
    CreateCourseCommand command,
    IMediator mediator)
{
    var result = await mediator.Send(command);
    return result.IsSuccess 
        ? Results.Created($"/api/courses/{result.Data.CourseId}", result)
        : Results.BadRequest(result);
}
```

### Step 6: Write Tests
```csharp
// Tests/CreateCourseCommandHandlerTests.cs
public class CreateCourseCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesCoursSuccessfully()
    {
        // Arrange
        var command = new CreateCourseCommand("Test Course EN", "Curso Test ES", ...);
        var handler = new CreateCourseCommandHandler(inMemoryDb);
        
        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
```

---

## 🔌 Dependency Injection

All features are registered in `Program.cs`:

```csharp
// Program.cs
builder.Services
    .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly))
    .AddFluentValidationAutoValidation()
    .AddScoped<IEmailService, FakeEmailService>()
    .AddScoped<IPasswordHasher, BcryptPasswordHasher>();
```

---

## 🚫 Anti-Patterns to Avoid

| Anti-Pattern | Why It's Bad | Correct Approach |
|---|---|---|
| Shared service logic | Tight coupling | Feature-owned logic |
| Mixing entities + DTOs | Unclear contracts | Always use DTOs for API |
| Exception-based control | Hard to test | Result<T> pattern |
| Hardcoded table names | Not maintainable | Use EF Core configurations |
| Magic strings | No IDE support | Use constants |
| Untested handlers | Unreliable code | Always test |

---

## 📝 Entity Configuration Best Practices

Entity configurations live in `Data/Configurations/` and configure:
- Table names (via `builder.ToTable("TableName")`)
- Indexes (`.HasIndex(...)`)
- Constraints (`.IsRequired()`)
- Relationships (`.HasMany().WithOne()`)
- Soft delete filter (`.HasQueryFilter()`)

Example:
```csharp
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Title).IsRequired().HasMaxLength(255);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        
        builder.HasQueryFilter(x => !x.IsDeleted);
        
        builder.HasMany(x => x.Topics)
            .WithOne()
            .HasForeignKey(x => x.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

---

## 🧪 Testing Architecture

Tests live alongside features:

```
/Modules/Auth/Register/Tests/
  ├── RegisterCommandHandlerTests.cs
  ├── RegisterValidatorTests.cs
  └── RegisterEndpointTests.cs
```

Each test class:
- Uses xUnit
- Uses FluentAssertions
- Creates test data using Builders or Fixtures
- Uses InMemory DB for integration tests

---

## 📊 Database Context

`ApplicationDbContext` is responsible for:
- Entity mapping (via configurations)
- Migration management
- Soft delete filtering
- Auditing interceptors

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Course> Courses { get; set; }
    // ...
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(builder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Audit logic here
        return await base.SaveChangesAsync(ct);
    }
}
```

---

## 🎯 Code Quality Checklist

Before committing a feature:
- [ ] Follows vertical slice structure
- [ ] Has dedicated Command + Validator + Handler
- [ ] Unit tests with 80%+ coverage
- [ ] No hardcoded strings (use constants)
- [ ] No SQL injection (uses EF Core)
- [ ] Domain rules documented in comments
- [ ] Endpoints have Swagger documentation
- [ ] Error messages support multilenguaje
- [ ] No shared DTOs between slices
- [ ] Async/await properly handled

---

## 🔗 Related Documents

- **AGENTS.md** - Overall vision + decisions
- **API_DESIGN.md** - Endpoint contracts
- **DOMAIN_MODEL.md** - Entity definitions
- **TESTING.md** - Testing strategy + examples
- **DATABASE.md** - EF Core + migrations

---

*This architecture enables boring, testable, scalable code.*
