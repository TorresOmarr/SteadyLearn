# Testing Guide

## Philosophy

> "Test behavior, not implementation"

- Test what users care about (business rules)
- Don't test implementation details
- Use real dependencies (InMemory DB)
- 80%+ coverage on critical paths

## Testing Pyramid

```
        /\           Integration (20%)
       /  \          - Full feature flows
      /    \         - API endpoints
     /______\
    /\      /\       Unit Tests (80%)
   /  \    /  \      - Handlers
  /    \  /    \     - Validators
 /______\/______\    - Domain rules
```

## What to Test

### Always Test
- Command/Query handlers
- Validators
- Domain rules
- Error scenarios

### Don't Test
- EF Core (trust Microsoft)
- Third-party libraries
- Trivial getters/setters

## Tools

```
xUnit                           - Test framework
FluentAssertions                - Assertions
Microsoft.EntityFrameworkCore.InMemory - Testing DB
```

## Project Structure

```
api.Tests/
├── Modules/
│   ├── Auth/
│   │   └── Register/
│   │       ├── RegisterCommandHandlerTests.cs
│   │       └── RegisterValidatorTests.cs
│   └── Courses/
├── Common/
│   ├── Fixtures/
│   │   └── DatabaseFixture.cs
│   └── Helpers/
└── api.Tests.csproj
```

## DatabaseFixture

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public ApplicationDbContext Context { get; private set; }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Context = new ApplicationDbContext(options);
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync() => await Context.DisposeAsync();
}
```

## Test Data Builder

```csharp
public class UserBuilder
{
    private string _email = "user@example.com";
    private UserRole _role = UserRole.Student;

    public UserBuilder WithEmail(string email) { _email = email; return this; }
    public UserBuilder AsAdmin() { _role = UserRole.Admin; return this; }

    public User Build() => new User
    {
        Id = Guid.NewGuid(),
        Email = _email,
        Role = _role,
        IsEmailVerified = true,
        CreatedAtUtc = DateTime.UtcNow
    };
}
```

## Handler Test Example

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

    public async Task DisposeAsync() => await _fixture.DisposeAsync();

    [Fact]
    public async Task Handle_WithValidCommand_CreatesCourse()
    {
        // Arrange
        var command = new CreateCourseCommand("EN Title", "ES Title", "EN Desc", "ES Desc");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.CourseId.Should().NotBeEmpty();
        
        var course = await _fixture.Context.Courses.FindAsync(result.Data.CourseId);
        course.Should().NotBeNull();
    }
}
```

## Validator Test Example

```csharp
public class CreateCourseValidatorTests
{
    private CreateCourseCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ReturnsSuccess()
    {
        var command = new CreateCourseCommand("Valid", "Válido", "Desc", "Desc");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyTitle_ReturnsFailure(string title)
    {
        var command = new CreateCourseCommand(title, "ES", "Desc", "Desc");
        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.PropertyName == "TitleEn");
    }
}
```

## Naming Convention

```
[UnitUnderTest]_[Scenario]_[ExpectedResult]

Examples:
- CreateCourseHandler_WithValidCommand_ReturnsCourseId
- CourseValidator_WithEmptyTitle_ReturnsError
- Course_CanPublish_WithoutTopics_ReturnsFalse
```

## Coverage Targets

| Module | Target |
|--------|--------|
| Auth (Register, Login) | 90%+ |
| Auth (Refresh, Verify) | 85%+ |
| Courses CRUD | 85%+ |
| Domain Rules | 100% |

### Measure Coverage

```bash
dotnet test /p:CollectCoverage=true
```

## Best Practices

### Do
- Descriptive test names
- Test one behavior per test
- Use builders for test data
- Test error scenarios
- Verify DB state changes
- Keep tests independent

### Don't
- Test multiple behaviors in one test
- Share state between tests
- Test implementation details
- Over-mock
- Use Thread.Sleep()

## FluentAssertions Cheat Sheet

```csharp
result.IsSuccess.Should().BeTrue();
result.Data.Should().NotBeNull();
items.Should().HaveCount(5);
string.Should().Contain("substring");
action.Should().ThrowAsync<Exception>();
```

## xUnit Attributes

```csharp
[Fact]              // Single test
[Theory]            // Parameterized
[InlineData]        // Inline parameters
[MemberData]        // Data from property
[IAsyncLifetime]    // Async setup/teardown
```

## Related Documentation

- [Backend Architecture](../backend/ARCHITECTURE.md) - Test structure
- [Domain Model](../backend/DOMAIN.md) - What to test
