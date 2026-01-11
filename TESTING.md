# 🧪 TESTING.md - Testing Strategy & Examples

**Status**: Foundation Phase
**Last Updated**: January 10, 2025

---

## 📌 Testing Philosophy

**Principle**: "Test behavior, not implementation"

- Test what users care about (business rules)
- Don't test implementation details
- Use real dependencies (InMemory DB, not mocks)
- Aim for 80%+ coverage on critical paths
- Make tests readable and maintainable

---

## 🏗️ Testing Pyramid

```
            /\
           /  \        Integration (20%)
          /    \       - Full feature flows
         /______\      - API endpoints
        /\      /\     - Handlers + DB
       /  \    /  \
      /    \  /    \   Unit Tests (80%)
     /______\/______\  - Handlers
    /\      /\      /\ - Validators
   /  \    /  \    /  \- Domain rules
  /    \  /    \  /    \
 /______\/______\/______\
```

---

## 🎯 What to Test

### ✅ ALWAYS TEST
```
1. Command/Query handlers (business logic)
2. Validators (input validation)
3. Domain rules (core business rules)
4. Error scenarios (failure paths)
```

### ⚠️ MAYBE TEST
```
1. Controllers/Endpoints (if complex logic)
2. DTOs (rarely needed)
3. Simple data mapping (not needed)
```

### ❌ DON'T TEST
```
1. EF Core (trust Microsoft)
2. Third-party libraries (trust the library)
3. Trivial getters/setters (waste of time)
4. Implementation details (encourages brittle tests)
```

---

## 🛠️ Tools & Setup

### NuGet Packages Required
```
xUnit                           - Test framework
xUnit.Runner.VisualStudio       - Test runner
FluentAssertions                - Assertions
Microsoft.EntityFrameworkCore.InMemory - Testing DB
```

### Test Project Structure
```
api.Tests/
├── Modules/
│   ├── Auth/
│   │   └── Register/
│   │       ├── RegisterCommandHandlerTests.cs
│   │       └── RegisterValidatorTests.cs
│   └── Courses/
│       └── CreateCourse/
│           ├── CreateCourseCommandHandlerTests.cs
│           └── CreateCourseValidatorTests.cs
├── Common/
│   ├── Fixtures/
│   │   ├── DatabaseFixture.cs (InMemory setup)
│   │   └── UserBuilder.cs (test data)
│   └── Helpers/
│       └── AssertionHelpers.cs
└── api.Tests.csproj
```

---

## 🧩 Test Setup Pattern

### DatabaseFixture (Reusable)
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    public ApplicationDbContext Context { get; private set; }

    public DatabaseFixture()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    public async Task InitializeAsync()
    {
        Context = new ApplicationDbContext(_options);
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.DisposeAsync();
    }

    public void ClearDatabase()
    {
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }
}
```

### Builder Pattern for Test Data
```csharp
public class UserBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _email = "user@example.com";
    private string _passwordHash = "hashed_password";
    private UserRole _role = UserRole.Student;

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder AsAdmin()
    {
        _role = UserRole.Admin;
        return this;
    }

    public User Build()
    {
        return new User
        {
            Id = _id,
            Email = _email,
            PasswordHash = _passwordHash,
            Role = _role,
            IsEmailVerified = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
```

---

## 📝 Test Examples

### Unit Test: Command Handler
```csharp
public class CreateCourseCommandHandlerTests : IAsyncLifetime
{
    private DatabaseFixture _fixture;
    private CreateCourseCommandHandler _handler;

    public async Task InitializeAsync()
    {
        _fixture = new DatabaseFixture();
        await _fixture.InitializeAsync();
        _handler = new CreateCourseCommandHandler(_fixture.Context);
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesCourseSuccessfully()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "Course EN",
            "Curso ES",
            "Description EN",
            "Descripción ES"
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.CourseId.Should().NotBeEmpty();

        // Verify in DB
        var course = await _fixture.Context.Courses
            .FirstOrDefaultAsync(x => x.Id == result.Data.CourseId);
        course.Should().NotBeNull();
        course!.IsPublished.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ReturnsFailure()
    {
        // Arrange
        var command = new CreateCourseCommand("", "", "", "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("VALIDATION_FAILED");
    }
}
```

### Unit Test: Validator
```csharp
public class CreateCourseValidatorTests
{
    private CreateCourseCommandValidator _validator;

    public CreateCourseValidatorTests()
    {
        _validator = new CreateCourseCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ReturnsSuccess()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "Valid Title EN",
            "Título Válido ES",
            "Description",
            "Descripción"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyTitleEn_ReturnsFalure(string titleEn)
    {
        // Arrange
        var command = new CreateCourseCommand(
            titleEn,
            "Título ES",
            "Description",
            "Descripción"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "TitleEn");
    }

    [Fact]
    public void Validate_WithMaxLengthExceeded_ReturnsFailure()
    {
        // Arrange
        var longTitle = new string('a', 256);
        var command = new CreateCourseCommand(
            longTitle,
            "Título ES",
            "Description",
            "Descripción"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
```

### Unit Test: Domain Rules
```csharp
public class CoursePublishingTests : IAsyncLifetime
{
    private DatabaseFixture _fixture;

    public async Task InitializeAsync()
    {
        _fixture = new DatabaseFixture();
        await _fixture.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public void CanPublish_WithoutTopics_ReturnsFalse()
    {
        // Arrange
        var course = new CourseBuilder().Build();

        // Act
        var canPublish = course.CanPublish();

        // Assert
        canPublish.Should().BeFalse();
    }

    [Fact]
    public void CanPublish_WithCompleteHierarchy_ReturnsTrue()
    {
        // Arrange
        var course = new CourseBuilder()
            .WithTopics(1)
            .WithSubtopicsPerTopic(1)
            .WithSessionsPerSubtopic(1)
            .Build();

        // Act
        var canPublish = course.CanPublish();

        // Assert
        canPublish.Should().BeTrue();
    }
}
```

### Integration Test: Full Flow
```csharp
public class RegisterUserIntegrationTests : IAsyncLifetime
{
    private DatabaseFixture _fixture;
    private IMediator _mediator;
    private IEmailService _emailService;

    public async Task InitializeAsync()
    {
        _fixture = new DatabaseFixture();
        await _fixture.InitializeAsync();

        // Setup mediator + services
        var services = new ServiceCollection()
            .AddMediatR(x => x.RegisterServicesFromAssembly(typeof(Program).Assembly))
            .AddFluentValidationAutoValidation()
            .AddScoped(_ => _fixture.Context)
            .AddScoped<IEmailService, FakeEmailService>();

        var provider = services.BuildServiceProvider();
        _mediator = provider.GetRequiredService<IMediator>();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task Register_ValidUser_CreatesUserAndSendsEmail()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com",
            "Password123!",
            "John",
            "Doe"
        );

        // Act
        var result = await _mediator.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify user in DB
        var user = await _fixture.Context.Users
            .FirstOrDefaultAsync(x => x.Email == "user@example.com");
        user.Should().NotBeNull();
        user!.IsEmailVerified.Should().BeFalse();
        user.EmailVerificationToken.Should().NotBeNull();

        // Verify email was sent (depends on IEmailService implementation)
    }
}
```

---

## 🎯 Test Naming Convention

```
[UnitUnderTest]_[Scenario]_[ExpectedResult]

Examples:
- CreateCourseCommandHandler_WithValidCommand_ReturnsCourseId
- CourseValidator_WithEmptyTitle_ReturnsValidationError
- Course_CanPublish_WithoutTopics_ReturnsFalse
```

---

## 📊 Coverage Guidelines

### Target Coverage by Module
```
Auth
  ├── Register: 90%+ (critical)
  ├── Login: 90%+ (critical)
  ├── RefreshToken: 85%+ (important)
  └── VerifyEmail: 85%+ (important)

Courses
  ├── Create: 85%+
  ├── Update: 85%+
  ├── Publish: 90%+ (business rule)
  └── Delete: 80%

Domain Rules
  └── All rules: 100% (non-negotiable)
```

### How to Measure
```bash
# Install Coverlet
dotnet add package coverlet.collector

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Generate HTML report
dotnet test /p:CollectCoverage=true \
  /p:CoverletOutputFormat=opencover \
  /p:CoverletOutput=./coverage.xml
```

---

## ⚡ Testing Best Practices

### ✅ DO
```
1. Use descriptive test names
2. Test one behavior per test
3. Use builders for test data
4. Test error scenarios
5. Verify state changes in DB
6. Keep tests independent
7. Use fake/mock services only for external APIs
```

### ❌ DON'T
```
1. Test multiple behaviors in one test
2. Share state between tests
3. Test implementation details
4. Over-mock (mock everything)
5. Ignore error cases
6. Use Thread.Sleep()
7. Hardcode test data
```

---

## 🧪 Testing Tools Reference

### xUnit Features
```csharp
[Fact]              // Single test
[Theory]            // Parameterized test
[InlineData]        // Pass inline parameters
[MemberData]        // Pass data from property/method
[ClassData]         // Pass complex test data
[IAsyncLifetime]    // Async setup/teardown
```

### FluentAssertions Examples
```csharp
result.IsSuccess.Should().BeTrue();
result.Data.CourseId.Should().NotBeEmpty();
items.Should().HaveCount(5);
string.Should().Contain("substring");
date.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
action.Should().ThrowAsync<InvalidOperationException>();
```

---

## 🔗 Related Documents

- **ARCHITECTURE.md** - Where tests live
- **AGENTS.md** - Overall testing philosophy
- **AUTH_IMPLEMENTATION.md** - Auth-specific tests
- **DOMAIN_MODEL.md** - Domain rule tests

---

*Tests are the safety net that enables confident refactoring.*
