# Backend Architecture

## Folder Structure

```
/api
├── src/
│   ├── Modules/                    # Feature slices
│   │   ├── Auth/
│   │   │   ├── Register/
│   │   │   ├── Login/
│   │   │   ├── Logout/
│   │   │   ├── RefreshToken/
│   │   │   ├── VerifyEmail/
│   │   │   └── ResetPassword/
│   │   ├── Courses/
│   │   ├── Topics/
│   │   ├── Subtopics/
│   │   └── Sessions/
│   ├── Common/                     # Shared utilities
│   │   ├── Abstractions/
│   │   │   ├── Messaging/          # CQRS interfaces
│   │   │   ├── Result.cs
│   │   │   └── Error.cs
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs
│   │   ├── Security/
│   │   │   ├── PasswordHasher.cs
│   │   │   ├── JwtTokenProvider.cs
│   │   │   ├── RefreshTokenService.cs
│   │   │   └── EmailService.cs
│   │   ├── Extensions/
│   │   └── Constants/
│   │       └── ErrorCodes.cs
│   ├── Domain/                     # Domain model
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── RefreshToken.cs
│   │   │   └── UserRole.cs
│   │   └── Interfaces/
│   │       └── IAuditableEntity.cs
│   ├── Data/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs
│   │   │   └── RefreshTokenConfiguration.cs
│   │   ├── Migrations/
│   │   └── AdminSeeder.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
└── docker-compose.yml
```

## Feature Slice Anatomy

Each feature follows this structure:

```
/Modules/{Module}/{Feature}/
├── Command.cs           # Input contract
├── CommandHandler.cs    # Business logic
├── CommandValidator.cs  # FluentValidation rules
└── Endpoint.cs          # HTTP endpoint mapping
```

### 1. Command/Query

Represents the input contract. Contains only data, no behavior.

```csharp
public record CreateCourseCommand(
    string TitleEn,
    string TitleEs,
    string DescriptionEn,
    string DescriptionEs
) : ICommand<CreateCourseResponse>;
```

### 2. Validator

FluentValidation rules for the command.

```csharp
public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(x => x.TitleEn).NotEmpty();
        RuleFor(x => x.TitleEs).NotEmpty();
    }
}
```

### 3. Handler

Contains application orchestration. Domain mutations must be in the model (aggregate) methods, not inline property sets.

```csharp
public class CreateCourseCommandHandler : ICommandHandler<CreateCourseCommand, CreateCourseResponse>
{
    private readonly ApplicationDbContext _db;
    
    public async Task<Result<CreateCourseResponse>> Handle(CreateCourseCommand request, CancellationToken ct)
    {
        var course = Course.Create(request.TitleEn, request.TitleEs /* ... */);
        // Mutations go through domain methods, not direct setters.
        _db.Courses.Add(course);
        await _db.SaveChangesAsync(ct);
        
        return Result.Success(new CreateCourseResponse(course.Id));
    }
}
```

### 4. Endpoint

Maps HTTP request to command/handler.

```csharp
public static class CreateCourseEndpoint
```

---

## Vertical Slice Playbook (backend)
- Command/Query: datos solo; sin comportamiento.
- Validator: FluentValidation sobre el command/query.
- Handler: orquestación y llamadas al dominio; no muta propiedades directas, usa métodos del modelo/aggregate.
- Endpoint: mapea HTTP → command/query → handler.
- Dominio: setters privados, factorías estáticas descriptivas, métodos de mutación (behaviors) dentro de la entidad.


### 4. Endpoint

Maps HTTP request to command/handler.

```csharp
public static class CreateCourseEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/courses", async (CreateCourseCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? Results.Created($"/api/courses/{result.Data.CourseId}", result)
                : Results.BadRequest(result);
        })
        .WithName("CreateCourse")
        .RequireAuthorization("AdminOnly");
    }
}
```

## CQRS Abstractions

Custom abstractions over MediatR for semantic clarity.

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     MediatR Layer                            │
│  IRequest<Result<T>> / IRequestHandler<TRequest, Result<T>> │
└─────────────────────────────────────────────────────────────┘
                              ▲
┌─────────────────────────────────────────────────────────────┐
│                  Our Abstractions Layer                      │
│    ICommand<T> / IQuery<T>                                  │
│    ICommandHandler<TCommand, TResponse>                     │
│    IQueryHandler<TQuery, TResponse>                         │
└─────────────────────────────────────────────────────────────┘
                              ▲
┌─────────────────────────────────────────────────────────────┐
│                   Feature Modules                            │
│  RegisterCommand : ICommand<RegisterResponse>               │
│  RegisterCommandHandler : ICommandHandler<...>              │
└─────────────────────────────────────────────────────────────┘
```

### Interfaces

**Commands (Write Operations)**:
```csharp
public interface IBaseCommand { }
public interface ICommand : IRequest<Result>, IBaseCommand { }
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand { }
```

**Command Handlers**:
```csharp
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand { }

public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse> { }
```

**Queries (Read Operations)**:
```csharp
public interface IQuery<TResponse> : IRequest<Result<TResponse>> { }

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse> { }
```

### Current Auth Implementation

| Feature | Command | Handler |
|---------|---------|---------|
| Register | `RegisterCommand : ICommand<RegisterResponse>` | `RegisterCommandHandler` |
| Login | `LoginCommand : ICommand<LoginResponse>` | `LoginCommandHandler` |
| VerifyEmail | `VerifyEmailCommand : ICommand<VerifyEmailResponse>` | `VerifyEmailCommandHandler` |
| RefreshToken | `RefreshTokenCommand : ICommand<RefreshTokenResponse>` | `RefreshTokenCommandHandler` |
| Logout | `LogoutCommand : ICommand<LogoutResponse>` | `LogoutCommandHandler` |
| RequestPasswordReset | `RequestPasswordResetCommand : ICommand<...>` | `RequestPasswordResetCommandHandler` |
| CompletePasswordReset | `CompletePasswordResetCommand : ICommand<...>` | `CompletePasswordResetCommandHandler` |

## Result Pattern

Explicit error handling without exceptions.

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public Error? Error { get; }
    
    public static Result<T> Success(T data);
    public static Result<T> Failure(string code, string? message = null);
}

public record Error(string Code, string? Message = null);
```

**Usage**:
```csharp
// Success
return Result.Success(new RegisterResponse { UserId = user.Id });

// Failure
return Result.Failure<RegisterResponse>(ErrorCodes.EMAIL_ALREADY_EXISTS);
```

## MediatR Pipeline

```
Request → ValidationBehavior → Handler → Response
```

The `ValidationBehavior` automatically validates commands before the handler runs:

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(TRequest request, ...)
    {
        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
            
        if (failures.Any())
            return CreateValidationFailureResult(failures);
            
        return await next();
    }
}
```

## Dependency Injection

All features registered in `Program.cs`:

```csharp
builder.Services
    .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly))
    .AddValidatorsFromAssembly(typeof(Program).Assembly)
    .AddScoped<IEmailService, FakeEmailService>()
    .AddScoped<IPasswordHasher, PasswordHasher>()
    .AddScoped<IJwtTokenProvider, JwtTokenProvider>();
```

## Code Quality Checklist

Before completing a feature:

- [ ] Follows vertical slice structure
- [ ] Has Command + Validator + Handler
- [ ] Unit tests written
- [ ] No hardcoded strings (use ErrorCodes)
- [ ] Uses Result pattern
- [ ] Endpoints have Swagger documentation
- [ ] Error messages support multilingual (via codes)
- [ ] No shared DTOs between slices

## Related Documentation

- [Project Decisions](../project/DECISIONS.md) - Why we chose this architecture
- [API Reference](./API.md) - Endpoint documentation
- [Domain Model](./DOMAIN.md) - Entity definitions
- [Database](./DATABASE.md) - EF Core patterns
