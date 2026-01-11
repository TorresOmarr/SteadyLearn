# 🧬 DOMAIN_MODEL.md - Entities & Domain Rules

**Status**: Foundation Phase
**Last Updated**: January 10, 2025

---

## 📌 Overview

The domain model defines the core entities and their relationships. Each entity:
- Has a unique GUID identifier
- Inherits from `IAuditableEntity` (tracks creation/modification)
- Supports soft delete (IsDeleted flag)
- Has translation tables for multilenguaje support

---

## 👤 User Entity

The User entity represents system users with role-based access.

### Properties
```
User
├── Id: Guid (PK)
├── Email: string (unique, indexed)
├── FirstName: string
├── LastName: string
├── PasswordHash: string (bcrypt hashed)
├── Role: UserRole enum (Admin | Student)
├── IsEmailVerified: bool
├── EmailVerificationToken: string? (24h expiry)
├── EmailVerificationTokenExpiresAt: DateTime?
├── PasswordResetToken: string? (24h expiry)
├── PasswordResetTokenExpiresAt: DateTime?
├── RefreshTokenHash: string? (HttpOnly cookie)
├── RefreshTokenExpiresAt: DateTime?
├── CreatedAtUtc: DateTime (audit)
├── CreatedBy: string (audit)
├── UpdatedAtUtc: DateTime (audit)
├── UpdatedBy: string (audit)
└── IsDeleted: bool (soft delete)
```

### Domain Rules
```
1. Email must be unique across system
2. Email is case-insensitive for login
3. Password must be at least 8 characters with:
   - 1 uppercase letter
   - 1 lowercase letter
   - 1 digit
   - 1 special character
4. Email verification tokens expire after 24 hours
5. Password reset tokens expire after 24 hours
6. Refresh tokens rotate on each use (old token invalidated)
7. Only Admin can create courses
8. Cannot delete own user account
```

### Related Data
```
User
├── Courses (many) - courses created by this user
├── Audit trails - who created/modified entities
└── [future] Student enrollment records
```

---

## 📚 Course Entity

Represents a course that admins create and manage.

### Properties
```
Course
├── Id: Guid (PK)
├── IsPublished: bool (default: false)
├── TotalDurationMinutes: int (calculated from sessions)
├── CreatedAtUtc: DateTime (audit)
├── CreatedBy: string (audit) - references User.Email
├── UpdatedAtUtc: DateTime (audit)
├── UpdatedBy: string (audit)
├── IsDeleted: bool (soft delete)
│
├── Relationships:
│   ├── CourseTranslations: List<CourseTranslation> (EN, ES)
│   ├── Topics: List<Topic>
│   └── [future] StudentEnrollments
└── [future] Tags, Categories, Prerequisites
```

### CourseTranslation Entity

Stores localized course content (EN/ES).

```
CourseTranslation
├── Id: Guid (PK)
├── CourseId: Guid (FK)
├── LanguageCode: string (en, es)
├── Title: string
├── Description: string
└── Unique constraint: (CourseId, LanguageCode)
```

### Domain Rules
```
1. Courses are created unpublished (IsPublished = false)
2. Can only be published if:
   - Has at least 1 topic
   - Each topic has at least 1 subtopic
   - Each subtopic has at least 1 session
3. TotalDurationMinutes calculated as SUM of all session durations
4. Course cannot be deleted if published (business rule decision)
5. Can only be edited by creator or admin
6. Translation required for both EN and ES before publishing
```

### Related Data
```
Course (1)
├── Topics (many) - 1..N relationship
├── CourseTranslations (exactly 2: EN, ES)
└── CreatedBy (User entity reference via email)
```

---

## 🏷️ Topic Entity

Represents a major section within a course.

### Properties
```
Topic
├── Id: Guid (PK)
├── CourseId: Guid (FK - cannot change)
├── Order: int (explicit ordering, no gaps)
├── CreatedAtUtc: DateTime (audit)
├── CreatedBy: string (audit)
├── UpdatedAtUtc: DateTime (audit)
├── UpdatedBy: string (audit)
├── IsDeleted: bool (soft delete)
│
├── Relationships:
│   ├── Course: Course (parent)
│   ├── TopicTranslations: List<TopicTranslation>
│   ├── Subtopics: List<Subtopic>
│   └── [future] LearningObjectives
└── Indexes:
    └── (CourseId, Order) - for ordering queries
```

### TopicTranslation Entity

```
TopicTranslation
├── Id: Guid (PK)
├── TopicId: Guid (FK)
├── LanguageCode: string (en, es)
├── Title: string
└── Unique constraint: (TopicId, LanguageCode)
```

### Domain Rules
```
1. Order is explicit (1, 2, 3, ...) with no gaps
2. Cannot move to different course (CourseId is immutable)
3. Deleting a topic cascades to subtopics and sessions
4. Order is auto-calculated when inserting
   - New topic gets Order = MAX(existing orders) + 1
5. Reordering requires updating other topics' order values
6. Title required in both EN and ES
```

### Related Data
```
Topic (1)
├── Course (1) - parent
├── Subtopics (many) - 1..N relationship
└── TopicTranslations (exactly 2: EN, ES)
```

---

## 🔖 Subtopic Entity

Represents a section within a topic.

### Properties
```
Subtopic
├── Id: Guid (PK)
├── TopicId: Guid (FK - cannot change)
├── Order: int (explicit ordering)
├── CreatedAtUtc: DateTime (audit)
├── CreatedBy: string (audit)
├── UpdatedAtUtc: DateTime (audit)
├── UpdatedBy: string (audit)
├── IsDeleted: bool (soft delete)
│
├── Relationships:
│   ├── Topic: Topic (parent)
│   ├── SubtopicTranslations: List<SubtopicTranslation>
│   ├── Sessions: List<Session>
│   └── [future] Quizzes
└── Indexes:
    └── (TopicId, Order)
```

### SubtopicTranslation Entity

```
SubtopicTranslation
├── Id: Guid (PK)
├── SubtopicId: Guid (FK)
├── LanguageCode: string (en, es)
├── Title: string
└── Unique constraint: (SubtopicId, LanguageCode)
```

### Domain Rules
```
1. Order is explicit with no gaps
2. Cannot move to different topic (TopicId is immutable)
3. Deleting a subtopic cascades to sessions
4. Sessions CAN move between subtopics
5. Order is auto-calculated on insert
6. Title required in both EN and ES
```

### Related Data
```
Subtopic (1)
├── Topic (1) - parent
├── Sessions (many) - 1..N, but sessions can move
└── SubtopicTranslations (exactly 2: EN, ES)
```

---

## 📖 Session Entity

Represents a single learning unit (video, markdown content, etc.).

### Properties
```
Session
├── Id: Guid (PK)
├── SubtopicId: Guid (FK)
├── Order: int (explicit ordering)
├── VideoUrl: string? (optional, any video host)
├── ContentMarkdown: string? (optional, supports markdown)
├── DurationMinutes: int? (optional, for TotalDuration calc)
├── DocumentationSource: string? (visible reference, unused)
├── CreatedAtUtc: DateTime (audit)
├── CreatedBy: string (audit)
├── UpdatedAtUtc: DateTime (audit)
├── UpdatedBy: string (audit)
├── IsDeleted: bool (soft delete)
│
├── Relationships:
│   ├── Subtopic: Subtopic (parent)
│   ├── SessionTranslations: List<SessionTranslation>
│   └── [future] Exercises, Submissions
└── Indexes:
    └── (SubtopicId, Order)
```

### SessionTranslation Entity

```
SessionTranslation
├── Id: Guid (PK)
├── SessionId: Guid (FK)
├── LanguageCode: string (en, es)
├── Title: string
├── ContentMarkdown: string? (translatable content)
└── Unique constraint: (SessionId, LanguageCode)
```

### Domain Rules
```
1. Order is explicit with no gaps
2. Can move between ANY subtopics (not restricted to same topic)
3. Can have:
   - Video URL only
   - Content markdown only
   - Both (recommended)
   - Neither (placeholder)
4. DurationMinutes is optional
   - If null, not counted in TotalDuration
   - Calculated as SUM of all non-null durations up the hierarchy
5. DocumentationSource is informational only (not used yet)
6. Title required in both EN and ES
7. Deleting affects student progress tracking (future)
```

### Related Data
```
Session (1)
├── Subtopic (1) - parent, but can move
├── SessionTranslations (exactly 2: EN, ES)
└── [future] Submissions, Comments
```

---

## 🔄 Entity Relationships

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

---

## 🧮 Calculated Fields

### Course.TotalDurationMinutes
```
Calculated as:
  SUM(
    Session.DurationMinutes
    WHERE Session.IsDeleted = false
    AND Session.Subtopic.IsDeleted = false
    AND Session.Subtopic.Topic.IsDeleted = false
    AND Session.Subtopic.Topic.Course.Id = Course.Id
  )
```

---

## 🗂️ Audit & Soft Delete

All entities track:

```
IAuditableEntity interface
├── CreatedAtUtc: DateTime
├── CreatedBy: string (user email, not ID)
├── UpdatedAtUtc: DateTime
├── UpdatedBy: string (user email, not ID)
└── IsDeleted: bool

Soft Delete Strategy:
  - Data is NEVER physically deleted
  - IsDeleted = true marks as deleted
  - Global query filter: .Where(x => !x.IsDeleted)
  - Physical deletion only in exceptional cases (GDPR, etc.)
```

---

## 🌍 Translation Strategy

Each translatable entity has a corresponding Translation table:

```
Course ←→ CourseTranslation
Topic ←→ TopicTranslation
Subtopic ←→ SubtopicTranslation
Session ←→ SessionTranslation
```

### Translation Table Structure
```
[Entity]Translation
├── Id: Guid (PK)
├── [Entity]Id: Guid (FK)
├── LanguageCode: string (en, es)
├── [translatable properties]
└── Unique constraint: ([Entity]Id, LanguageCode)
```

### Loading Pattern
```
// Get course in English
var course = dbContext.Courses
    .Include(x => x.CourseTranslations.Where(t => t.LanguageCode == "en"))
    .FirstOrDefaultAsync();

// Returns:
// course.Title (from EN translation)
// course.Description (from EN translation)
```

---

## 📋 Constraints & Indexes

### Unique Constraints
```
User
  ├── Email (unique, case-insensitive)
  └── (case-insensitive index)

[Entity]Translation
  ├── ([Entity]Id, LanguageCode) - composite unique
  └── Ensures only 1 EN and 1 ES per entity
```

### Indexes
```
Course
  └── (IsDeleted, IsPublished)

Topic
  ├── (CourseId, Order)
  └── (CourseId, IsDeleted)

Subtopic
  ├── (TopicId, Order)
  └── (TopicId, IsDeleted)

Session
  ├── (SubtopicId, Order)
  └── (SubtopicId, IsDeleted)

User
  ├── Email (unique, case-insensitive)
  └── (Role, IsEmailVerified)
```

---

## 🚫 Immutable Fields

Once created, these fields cannot be changed:

```
User
  └── Email (CAN be changed via password reset flow)

Course
  └── None (all fields can be updated)

Topic
  └── CourseId (topics cannot move between courses)

Subtopic
  └── TopicId (subtopics cannot move between topics)

Session
  └── [none - can move between subtopics]
```

---

## 🔗 Related Documents

- **ARCHITECTURE.md** - How entities are structured in code
- **DATABASE.md** - EF Core configurations + migrations
- **API_DESIGN.md** - How entities are exposed via API
- **AGENTS.md** - Overall vision

---

*This domain model ensures data integrity and supports scalable growth.*
