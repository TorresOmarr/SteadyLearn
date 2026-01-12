# Domain Model

## Overview

All entities:
- Have a unique GUID identifier
- Inherit from `IAuditableEntity` (tracks creation/modification)
- Support soft delete (`IsDeleted` flag)
- Have translation tables for multilingual support (where applicable)
- Use private setters for properties; mutate state through methods
- Provide a descriptive static factory method for creation (e.g., `User.Create(...)`, `Course.Create(...)`)
- Use `DateTimeOffset` for time fields and persist as `timestamp with time zone`

## User Entity

```
User
├── Id: Guid (PK)
├── Email: string (unique, indexed)
├── FirstName: string
├── LastName: string
├── PasswordHash: string (bcrypt hashed)
├── Role: UserRole enum (Admin | Student)
├── IsEmailVerified: bool
├── EmailVerificationTokenHash: string? (expiry stored)
├── EmailVerificationTokenExpiresAt: DateTimeOffset?
├── PasswordResetTokenHash: string? (expiry stored)
├── PasswordResetTokenExpiresAt: DateTimeOffset?
├── CreatedAt: DateTimeOffset
├── UpdatedAt: DateTimeOffset?
├── DeletedAt: DateTimeOffset?
└── IsDeleted: bool
```

**Domain Rules**:
1. Email must be unique (case-insensitive)
2. Password: minimum 8 chars with uppercase, lowercase, digit, special char
3. Email verification tokens expire after 24 hours
4. Password reset tokens expire after 24 hours
5. Refresh tokens se manejan en tabla dedicada (ver Database); rotan en cada uso, 1 activo + 5 históricos
6. Solo Admin puede crear cursos

## Course Entity

```
Course
├── Id: Guid (PK)
├── IsPublished: bool (default: false)
├── TotalDurationMinutes: int (calculated from sessions)
├── CreatedAtUtc: DateTime
├── CreatedBy: string
├── UpdatedAtUtc: DateTime?
├── UpdatedBy: string?
├── IsDeleted: bool
└── Relationships:
    ├── CourseTranslations: List<CourseTranslation>
    └── Topics: List<Topic>
```

**CourseTranslation**:
```
CourseTranslation
├── Id: Guid (PK)
├── CourseId: Guid (FK)
├── LanguageCode: string (en, es)
├── Title: string
├── Description: string
└── Unique: (CourseId, LanguageCode)
```

---

### Encapsulation Pattern (example)
- Setters private.
- Factory for creation.
- Methods for behaviors/mutations.

```csharp
public class User : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public DateTimeOffset? EmailVerifiedAt { get; private set; }
    // ... other fields ...

    public static User Create(string email, string passwordHash, string? firstName = null, string? lastName = null, UserRole role = UserRole.Student)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLower().Trim(),
            PasswordHash = passwordHash,
            FirstName = firstName?.Trim(),
            LastName = lastName?.Trim(),
            Role = role,
            IsEmailVerified = false,
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };
    }

    public void MarkEmailAsVerified()
    {
        IsEmailVerified = true;
        EmailVerifiedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetProfile(string? firstName, string? lastName)
    {
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
```

**Domain Rules**:
1. Courses are created unpublished
2. Can only publish if: has topics → subtopics → sessions
3. TotalDurationMinutes = SUM of session durations
4. Translation required for EN and ES before publishing

## Topic Entity

```
Topic
├── Id: Guid (PK)
├── CourseId: Guid (FK - immutable)
├── Order: int (explicit ordering)
├── CreatedAtUtc: DateTime
├── CreatedBy: string
├── UpdatedAtUtc: DateTime?
├── UpdatedBy: string?
├── IsDeleted: bool
└── Relationships:
    ├── TopicTranslations: List<TopicTranslation>
    └── Subtopics: List<Subtopic>
```

**TopicTranslation**:
```
TopicTranslation
├── Id: Guid (PK)
├── TopicId: Guid (FK)
├── LanguageCode: string
├── Title: string
└── Unique: (TopicId, LanguageCode)
```

**Domain Rules**:
1. Order is explicit (1, 2, 3...) with no gaps
2. Cannot move to different course (CourseId is immutable)
3. Deleting cascades to subtopics and sessions

## Subtopic Entity

```
Subtopic
├── Id: Guid (PK)
├── TopicId: Guid (FK - immutable)
├── Order: int
├── CreatedAtUtc: DateTime
├── CreatedBy: string
├── UpdatedAtUtc: DateTime?
├── UpdatedBy: string?
├── IsDeleted: bool
└── Relationships:
    ├── SubtopicTranslations: List<SubtopicTranslation>
    └── Sessions: List<Session>
```

**Domain Rules**:
1. Cannot move to different topic
2. Deleting cascades to sessions
3. Sessions CAN move between subtopics

## Session Entity

```
Session
├── Id: Guid (PK)
├── SubtopicId: Guid (FK - can change via move)
├── Order: int
├── VideoUrl: string? (optional)
├── ContentMarkdown: string? (optional)
├── DurationMinutes: int? (optional)
├── DocumentationSource: string? (reference only)
├── CreatedAtUtc: DateTime
├── CreatedBy: string
├── UpdatedAtUtc: DateTime?
├── UpdatedBy: string?
├── IsDeleted: bool
└── Relationships:
    └── SessionTranslations: List<SessionTranslation>
```

**SessionTranslation**:
```
SessionTranslation
├── Id: Guid (PK)
├── SessionId: Guid (FK)
├── LanguageCode: string
├── Title: string
├── ContentMarkdown: string? (translatable)
└── Unique: (SessionId, LanguageCode)
```

**Domain Rules**:
1. Can move between ANY subtopics
2. Can have video, markdown, both, or neither
3. DurationMinutes is optional (not counted if null)

## Entity Relationships

```
Course (1)
├── CourseTranslation (2: EN, ES)
└── Topic (many)
    ├── TopicTranslation (2: EN, ES)
    └── Subtopic (many)
        ├── SubtopicTranslation (2: EN, ES)
        └── Session (many)
            └── SessionTranslation (2: EN, ES)

User
├── [audit] CreatedBy reference (email string)
└── [audit] UpdatedBy reference (email string)
```

## Auditable Entity Interface

```csharp
public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    string CreatedBy { get; set; }
    DateTime? UpdatedAtUtc { get; set; }
    string? UpdatedBy { get; set; }
    bool IsDeleted { get; set; }
}
```

**Audit Strategy**:
- Data is NEVER physically deleted
- `IsDeleted = true` marks as deleted
- Global query filter: `.Where(x => !x.IsDeleted)`
- Physical deletion only for GDPR compliance

## Calculated Fields

**Course.TotalDurationMinutes**:
```sql
SUM(Session.DurationMinutes)
WHERE Session.IsDeleted = false
  AND Session.Subtopic.IsDeleted = false
  AND Session.Subtopic.Topic.IsDeleted = false
  AND Session.Subtopic.Topic.Course.Id = Course.Id
```

## Immutable Fields

| Entity | Immutable Fields |
|--------|-----------------|
| User | Email (can change via reset flow) |
| Topic | CourseId |
| Subtopic | TopicId |
| Session | None (can move between subtopics) |

## Indexes

| Table | Index |
|-------|-------|
| User | Email (unique, case-insensitive) |
| User | (Role, IsEmailVerified) |
| Course | (IsDeleted, IsPublished) |
| Topic | (CourseId, Order) |
| Subtopic | (TopicId, Order) |
| Session | (SubtopicId, Order) |
| *Translation | (EntityId, LanguageCode) unique |

## Related Documentation

- [Backend Architecture](./ARCHITECTURE.md) - Code structure
- [Database](./DATABASE.md) - EF Core configurations
- [API Reference](./API.md) - How entities are exposed
