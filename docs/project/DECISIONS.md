# Architecture Decisions

Key decisions made for SteadyLearn and their rationale.

## 1. Vertical Slice Architecture

**Decision**: Organize code by feature (vertical slices), not by layer (horizontal).

**Why**:
- Each feature is self-contained and deletable
- No shared business logic between features
- Easy to test in isolation
- Clear ownership of each feature
- Scales well as the codebase grows

**Structure**:
```
/Modules/Auth/Register/
  ├── Command.cs
  ├── CommandHandler.cs
  ├── CommandValidator.cs
  └── Endpoint.cs
```

## 2. CQRS-lite Pattern

**Decision**: Separate commands (writes) from queries (reads) using custom abstractions over MediatR.

**Why**:
- Clear intent: `ICommand` vs `IQuery`
- Enforced Result pattern at compile time
- Can apply different behaviors to reads vs writes
- Better semantics than generic `IRequest`

**Pattern**:
```csharp
ICommand<TResponse>     // Write operations
IQuery<TResponse>       // Read operations
```

## 3. Result Pattern (No Exceptions for Business Logic)

**Decision**: Use `Result<T>` instead of exceptions for business logic failures.

**Why**:
- Explicit error handling
- Type-safe error codes
- Easier to test
- No hidden control flow
- Forces handling of failure cases

**Usage**:
```csharp
return Result.Success(data);
return Result.Failure<T>(ErrorCodes.SomeError, "message");
```

## 4. Separate Translation Tables

**Decision**: Each translatable entity has a corresponding `[Entity]Translation` table.

**Why**:
- Normalized data (no redundant columns)
- Scales to N languages (not just 2)
- Easy to query specific language
- Can filter incomplete translations
- Independent translation updates

**Pattern**:
```
Course ←→ CourseTranslation (EN, ES rows)
Topic  ←→ TopicTranslation
```

## 5. JWT Strategy: Access + Refresh Tokens

**Decision**: Short-lived access tokens (15 min) + HttpOnly refresh tokens (7 days).

**Why**:
- Access token: Stateless, sent in Authorization header
- Refresh token: HttpOnly cookie (XSS protection)
- Token rotation on each refresh (security)
- Balances security with UX

## 6. Language-Agnostic Error Codes

**Decision**: API returns error codes (`EMAIL_ALREADY_EXISTS`), frontend translates.

**Why**:
- Decouples API from translation logic
- No database bloat for error messages
- Frontend controls display language
- Easy to add new languages

**Response**:
```json
{
  "success": false,
  "error": {
    "code": "EMAIL_ALREADY_EXISTS"
  }
}
```

## 7. Fake Email Service in Development

**Decision**: Console log verification links instead of sending real emails.

**Why**:
- No external dependencies during dev
- Faster development cycle
- Easy to test verification flows
- Production uses real SMTP

## 8. Admin Seeding via Configuration

**Decision**: Create initial admin user from `appsettings.Development.json`.

**Why**:
- Reproducible setup
- No manual database manipulation
- Version-controlled defaults
- One-command local dev setup

## 9. Soft Delete Strategy

**Decision**: Never physically delete data, use `IsDeleted` flag with global query filters.

**Why**:
- Audit trails preserved
- Data recovery possible
- GDPR compliance (can hard delete if required)
- Prevents accidental data loss

## 10. PostgreSQL with Docker Compose

**Decision**: Single `docker-compose.yml` with PostgreSQL container.

**Why**:
- One-command local dev setup
- Consistent database across team
- Easy to reset and recreate
- Migrations run on startup

## 11. FluentValidation for Input Validation

**Decision**: Use FluentValidation with MediatR pipeline behavior.

**Why**:
- Chainable, reusable validators
- Easy to test
- Clear validation rules
- Automatic validation before handler execution

## 12. Auditable Entities

**Decision**: All entities track `CreatedAtUtc`, `CreatedBy`, `UpdatedAtUtc`, `UpdatedBy`.

**Why**:
- Accountability (who changed what)
- Debugging capability
- Audit compliance
- Auto-populated via DbContext override

## Anti-Patterns to Avoid

| Anti-Pattern | Why Bad | Correct Approach |
|--------------|---------|------------------|
| Shared service logic | Tight coupling | Feature-owned logic |
| Entities in API responses | Unclear contracts | Always use DTOs |
| Exception-based control flow | Hard to test | Result pattern |
| Hardcoded table names | Not maintainable | EF Core configurations |
| Magic strings | No IDE support | Use constants |
| Untested handlers | Unreliable | Always test |
| Hardcoded translation | Inflexible | Error codes + frontend |

## Related Documentation

- [Project Overview](./OVERVIEW.md) - Vision and scope
- [Backend Architecture](../backend/ARCHITECTURE.md) - Implementation details
- [CQRS Abstractions](../backend/ARCHITECTURE.md#cqrs-abstractions) - Command/Query patterns
