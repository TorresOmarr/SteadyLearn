# 🎯 AGENTS.md - SteadyLearn MVP Blueprint

**Last Updated**: January 10, 2025
**Status**: Foundation Phase
**Target**: Production-ready MVP by Q1 2025

---

## 📌 Executive Summary

**SteadyLearn** is a **boring, testable, scalable** educational platform MVP. We build the foundation that will survive long-term evolution and AI/automation additions without architectural debt.

- **Tech Stack**: .NET 8 (Backend), React 19 + TypeScript (Frontend), PostgreSQL
- **Architecture**: Vertical Slice Architecture (Modular Monolith)
- **Approach**: Domain-Driven Design principles + Test-Driven Development
- **Language Support**: English (primary), Spanish (from Day 1)

---

## 🎯 Scope: WHAT WE BUILD

### ✅ IN SCOPE
- Authentication (local email/password + JWT)
- Course creation and management (Admin only)
- Topics, Subtopics, Sessions hierarchy
- Manual content organization
- Multilenguaje support (EN/ES)
- RESTful API with Swagger
- Clean, testable code

### ❌ NOT IN SCOPE (Phase 1)
- AI/LLM integration
- Background jobs / Message queues
- Microservices / Multi-tenancy
- Feature flags
- Real-time collaboration
- Student features (Phase 2)
- Analytics / Metrics
- Payment systems

---

## 🚀 Sprints Overview

### **SPRINT 1: Authentication Foundation** (Week 1-2)
**Goal**: Enable secure user registration and login, seed admin user

**Deliverables**:
- ✅ User entity with roles (Admin, Student)
- ✅ Email verification flow (with mock email in dev)
- ✅ JWT tokens (Access + Refresh)
- ✅ Password hashing (bcrypt)
- ✅ Login/Register/Logout endpoints
- ✅ Admin seeding in docker-compose
- ✅ Auth middleware + policy-based authorization
- ✅ Login/Register UI (React)
- ✅ Zustand auth store

**Key Features**:
- Refresh token rotation (security best practice)
- Email verification tokens (24h expiry)
- Password reset flow
- HttpOnly cookies for refresh tokens

---

### **SPRINT 2: Course Management** (Week 3-4)
**Goal**: Enable admins to build courses with structure

**Deliverables**:
- ✅ Course entity + translation tables
- ✅ Topic, Subtopic, Session entities
- ✅ CRUD endpoints for each
- ✅ Ordering system (explicit Order field)
- ✅ Course publish/unpublish
- ✅ Course Builder UI (React)
  - Tree view + Accordion hybrid
  - Inline/modal editing
  - Delete confirmation dialogs

**Key Features**:
- Topics cannot move between courses
- Sessions can move between subtopics
- Sessions have optional video URL + markdown content
- Documentation source field (visible, not used yet)

---

## 🏗️ Architecture Overview

### Backend: Vertical Slice Architecture

```
/api
  /src
    /Modules
      /Auth
        /Register
          Endpoint.cs
          Command.cs
          CommandHandler.cs
          CommandValidator.cs
          Tests/
        /Login
        /Logout
        /RefreshToken
        /VerifyEmail
        /ResetPassword
      /Courses
        /CreateCourse
        /GetCourse
        /PublishCourse
        /DeleteCourse
        Tests/
      /Topics
      /Subtopics
      /Sessions
    /Common
      /Models
        Result<T>.cs
        ApiResponse.cs
      /Behaviors
        ValidationBehavior.cs
      /Middleware
        ErrorHandlingMiddleware.cs
        AcceptLanguageMiddleware.cs
        AuthMiddleware.cs
      /Extensions
        ServiceCollectionExtensions.cs
    /Data
      ApplicationDbContext.cs
      Migrations/
    /Domain
      Entities/
      Interfaces/
    appsettings.json
    appsettings.Development.json
    Program.cs
```

### Frontend: Feature-Based Structure

```
/client
  /src
    /features
      /auth
        /components
          LoginForm.tsx
          RegisterForm.tsx
        /hooks
          useAuth.ts
        /services
          authService.ts
      /courses
        /components
          CourseBuilder.tsx
          TopicForm.tsx
          SessionForm.tsx
        /hooks
          useCourses.ts
        /services
          courseService.ts
    /stores
      authStore.ts
      courseStore.ts
    /shared
      /components
        Layout.tsx
        Navigation.tsx
        LoadingSpinner.tsx
      /hooks
        useApi.ts
      /utils
        i18n.ts
        apiClient.ts
    /types
      index.ts
    App.tsx
    main.tsx
```

---

## 🔑 Critical Decisions

### 1. **Multilenguaje: Separate Translation Tables**
- Why: Normalized, easier to query, scales to N languages
- Impact: Each translatable entity has a `[Entity]Translation` table
- Example: `Course` + `CourseTranslation` (EN/ES rows)

### 2. **JWT Strategy: Access + Refresh Tokens**
- Access token: Short-lived (15 min), sent in Authorization header
- Refresh token: Long-lived, HttpOnly cookie, rotated on each use
- Why: Balances security (XSS protection) + UX (no logout redirects)

### 3. **Error Handling: Accept-Language Middleware**
- API returns error codes: `{"code": "EMAIL_ALREADY_EXISTS"}`
- Frontend translates using Accept-Language header
- Why: Decouples API from translation logic, no DB bloat

### 4. **Email Service: Fake/Mock in Development**
- Development: Console logs verification links
- Production: Real SMTP (configured later)
- Why: No external dependencies during dev, testable

### 5. **Admin Seeding: Database Seeding**
- Initial admin created via docker-compose entrypoint
- Credentials in `appsettings.Development.json`
- Why: Reproducible, no manual setup, version-controlled

### 6. **Database: Docker Compose**
- Single `docker-compose.yml` with PostgreSQL
- Migrations run automatically on startup
- Why: One-command local dev setup

---

## 📊 Data Model Overview

### Core Entities
```
User
├── Id (Guid)
├── Email
├── PasswordHash
├── Role (Admin | Student)
├── IsEmailVerified
└── Audit fields (CreatedAt, UpdatedAt, etc.)

Course
├── Id (Guid)
├── IsPublished
├── TotalDurationMinutes (calculated)
├── Topics[]
└── Audit fields

CourseTranslation (EN/ES)
├── CourseId
├── LanguageCode
├── Title
├── Description

Topic
├── Id
├── CourseId
├── Order
├── Subtopics[]

Subtopic
├── Id
├── TopicId
├── Order
├── Sessions[]

Session
├── Id
├── SubtopicId
├── Order
├── Title
├── ContentMarkdown
├── VideoUrl (optional)
├── DurationMinutes (optional)
├── DocumentationSource (reference only, unused)
```

---

## 🔄 Implementation Flow

### For Every Feature Slice:

```
1. DOMAIN FIRST
   - Define entity
   - Define domain rules
   - Create migration

2. API SECOND
   - Write command/query
   - Write validator
   - Write handler
   - Write endpoint
   - Add tests

3. FRONTEND THIRD
   - Write service
   - Write store (Zustand)
   - Write component
   - Test integration
```

### Example: Create Course (Sprint 2)
```
Backend:
  1. Create Course entity + CourseTranslation
  2. Write CreateCourseCommand + validator
  3. Write CreateCourseHandler (creates EN/ES translations)
  4. Write POST /courses endpoint
  5. Write unit tests

Frontend:
  1. Write courseService.createCourse()
  2. Add to courseStore
  3. Build CourseForm component
  4. Wire to Dashboard
  5. Test end-to-end
```

---

## 🧪 Testing Philosophy

**Principle**: "Test behavior, not implementation"

- **Unit Tests**: Handlers, validators, domain rules
- **Integration Tests**: API endpoints (in-memory DB or Testcontainers)
- **No mocking**: Use real EF Core InMemory for tests
- **Coverage Target**: 80%+ on critical paths

**Test Structure**:
```
Features/
  Auth/
    Register/
      RegisterCommandHandlerTests.cs
      RegisterValidatorTests.cs
    RegisterCommandHandlerTests.cs
```

---

## 🚦 Quality Gates

Before marking a feature as DONE:

- [ ] Code follows vertical slice pattern
- [ ] Validation error messages support EN/ES
- [ ] Unit tests pass (>80% coverage)
- [ ] Integration tests pass
- [ ] Swagger documentation updated
- [ ] No hardcoded strings (use i18n)
- [ ] No SQL injection (always use EF Core)
- [ ] Domain rules documented in code

---

## 📖 Quick Reference: Command Pattern

```
Feature: Register User

1. Create RegisterCommand (input)
2. Create RegisterCommandValidator
3. Create RegisterCommandHandler (business logic)
4. Register handler in DI
5. Create endpoint (Maps command → API response)
6. Create unit + integration tests

Result: Type-safe, testable, traceable from API to DB
```

---

## 🎓 Senior Patterns We Use

1. **Vertical Slices**: Each feature is self-contained, no shared logic
2. **Domain-Driven Design**: Entity boundaries, aggregate roots
3. **CQRS-lite**: Separate commands (writes) from queries (reads)
4. **Result<T>**: Explicit error handling, no exceptions for business logic
5. **Fluent Validation**: Chainable, reusable, testable validators
6. **Middleware**: Cross-cutting concerns (auth, errors, i18n)
7. **Soft Deletes**: Never lose data, audit trails
8. **Translation Tables**: Proper multilenguaje support

---

## ⚠️ Pitfalls to Avoid

- ❌ Hardcoded table names in entity config
- ❌ Mixing business logic in endpoints
- ❌ Using string exceptions for business logic
- ❌ Sharing DTOs between slices
- ❌ Async/await confusion (we're careful)
- ❌ Translation hardcoded in API responses
- ❌ Not testing validators
- ❌ Magic strings without constants

---

## 📋 How to Use This Document

**For the Agent/Developer**:
1. Read this file first (you're here)
2. Check ARCHITECTURE.md for backend structure
3. Check API_DESIGN.md for endpoint contracts
4. Check DOMAIN_MODEL.md for entities
5. Check specific feature doc (AUTH_IMPLEMENTATION.md, etc.)
6. Refer to TESTING.md for test patterns
7. Check SETUP.md to run locally

**When Adding a New Feature**:
1. Define domain rules (DOMAIN_MODEL.md)
2. Plan API endpoints (API_DESIGN.md)
3. Create vertical slice in backend
4. Write tests first (TESTING.md)
5. Implement handler
6. Create frontend feature
7. Update Swagger docs

---

## 🔗 Document Map

| Document | Purpose |
|----------|---------|
| **AGENTS.md** | This file - blueprint + decisions |
| **ARCHITECTURE.md** | Backend folder structure + patterns |
| **API_DESIGN.md** | REST endpoints + contracts |
| **DOMAIN_MODEL.md** | Entity definitions + rules |
| **TESTING.md** | Testing strategy + examples |
| **AUTH_IMPLEMENTATION.md** | Auth system details |
| **I18N_STRATEGY.md** | Multilenguaje approach |
| **DATABASE.md** | EF Core + migrations |
| **FRONTEND.md** | React structure + stores |
| **SETUP.md** | Getting started locally |

---

## 👥 Roles & Responsibilities

**Admin User** (Sprint 1 onwards):
- Register & login
- Create/edit/publish courses
- Manage course hierarchy

**Student User** (Sprint 3+):
- View published courses
- Track progress
- Take tests/assessments

---

## 📞 When in Doubt

1. **Is it unclear?** → Make the safest, simplest assumption
2. **Document it** → Add comment with rationale
3. **Ask for review** → Another senior dev should validate
4. **Code should be deletable** → If we need to remove it, no cascading breaks

---

## 🎬 Getting Started

```bash
# Clone and navigate
cd SteadyLearn

# Start PostgreSQL
docker-compose up -d

# Restore packages and run migrations
cd api
dotnet restore
dotnet ef database update

# Start API
dotnet run

# In another terminal, start Frontend
cd ../client
npm install
npm run dev

# Login with seeded admin user
Email: admin@example.com
Password: (check appsettings.Development.json)
```

---

**Remember**: "Another senior dev should thank me, not curse me."

Boring code > clever code. Tested code > untested code. Simple architecture > complex architecture.

---

*This document is the source of truth. Update it as we learn and evolve.*
